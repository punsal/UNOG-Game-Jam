--[[
ART-01 -- Static Traveller, production export.

Approved: B2-A Balanced (from chr_traveller_base_revision_b2.aseprite,
layer "B2_A_Balanced"). This script does NOT modify that revision file --
it opens it read-only, extracts the approved layer's pixel data by value,
and closes it without saving. It then writes a new, clean single-image
source and the flattened production PNG.

Produces:
  - Assets/Art/Source/Aseprite/chr_traveller_base.aseprite
  - Assets/Art/Production/Characters/chr_traveller_base.png
and prints an audit confirming the export is pixel-identical to the
approved B2-A artwork (no drift during extraction) plus the standard
palette/alpha checks.

Usage:
  "$ASEPRITE_BIN" -b --script Tools/Aseprite/export_art01_traveller_production.lua
--]]

local function scriptDir()
  local src = debug.getinfo(1, "S").source
  local path = src:match("^@(.*)$") or src
  return path:match("^(.*)[/\\][^/\\]+$")
end

local projectRoot = scriptDir() .. "/../.."
local sourceOutDir = projectRoot .. "/Assets/Art/Source/Aseprite"
local prodOutDir = projectRoot .. "/Assets/Art/Production/Characters"

os.execute(string.format("mkdir -p '%s'", sourceOutDir))
os.execute(string.format("mkdir -p '%s'", prodOutDir))

local revisionPath = sourceOutDir .. "/chr_traveller_base_revision_b2.aseprite"
local newSourcePath = sourceOutDir .. "/chr_traveller_base.aseprite"
local prodPngPath = prodOutDir .. "/chr_traveller_base.png"

local function checkOverwrite(path)
  if app.fs.isFile(path) then
    print("NOTICE: overwriting existing file: " .. path)
  end
end

local W, H = 24, 24
local BONE  = app.pixelColor.rgba(217, 225, 232, 255)
local STONE = app.pixelColor.rgba(42, 49, 66, 255)
local NAVY  = app.pixelColor.rgba(18, 27, 45, 255)
local CLEAR = app.pixelColor.rgba(0, 0, 0, 0)
local GUIDE_BASELINE = app.pixelColor.rgba(255, 0, 255, 255)
local GUIDE_CENTER   = app.pixelColor.rgba(0, 255, 255, 255)
local PALETTE_HEX = { ["#D9E1E8"] = true, ["#2A3142"] = true, ["#121B2D"] = true }

----------------------------------------------------------------------
-- Read the approved layer from the revision file (read-only)
----------------------------------------------------------------------

if not app.fs.isFile(revisionPath) then
  error("Revision source not found: " .. revisionPath)
end

local revisionSpr = app.open(revisionPath)
local approvedLayer = nil
for _, layer in ipairs(revisionSpr.layers) do
  if layer.name == "B2_A_Balanced" then approvedLayer = layer end
end
if approvedLayer == nil then
  error("Layer 'B2_A_Balanced' not found in " .. revisionPath)
end

local approvedCel = approvedLayer:cel(1)
local approvedImg = approvedCel.image:clone()

revisionSpr:close() -- discard without saving; revision file on disk is untouched

----------------------------------------------------------------------
-- Build the clean single-image production source
----------------------------------------------------------------------

local spr = Sprite(W, H, ColorMode.RGB)
spr.filename = newSourcePath

local pal = Palette(4)
pal:setColor(0, Color{r=0, g=0, b=0, a=0})
pal:setColor(1, Color{r=217, g=225, b=232, a=255})
pal:setColor(2, Color{r=42, g=49, b=66, a=255})
pal:setColor(3, Color{r=18, g=27, b=45, a=255})
spr:setPalette(pal)

local artLayer = spr.layers[1]
artLayer.name = "TravellerBase"
local artCel = artLayer:cel(1)
if artCel == nil then artCel = spr:newCel(artLayer, 1) end
artCel.image = approvedImg

local guidesLayer = spr:newLayer()
guidesLayer.name = "Guides"
local guidesImg = Image(W, H, ColorMode.RGB)
for y = 0, H - 1 do
  for x = 0, W - 1 do
    guidesImg:drawPixel(x, y, CLEAR)
  end
end
for x = 0, W - 1 do guidesImg:drawPixel(x, 21, GUIDE_BASELINE) end
for y = 0, H - 1 do guidesImg:drawPixel(12, y, GUIDE_CENTER) end
local guidesCel = spr:newCel(guidesLayer, 1)
guidesCel.image = guidesImg
guidesLayer.isVisible = false

artLayer.isVisible = true

checkOverwrite(newSourcePath)
spr:saveAs(newSourcePath)
print("Saved source: " .. newSourcePath)

----------------------------------------------------------------------
-- Production PNG export (art layer only, guides hidden)
----------------------------------------------------------------------

guidesLayer.isVisible = false
artLayer.isVisible = true
checkOverwrite(prodPngPath)
spr:saveCopyAs(prodPngPath)
print("Exported: " .. prodPngPath)

----------------------------------------------------------------------
-- Audit: pixel-identity vs approved source, plus standard checks
----------------------------------------------------------------------

local mismatches = 0
local visibleCount, partialCount, outOfPaletteCount = 0, 0, 0
local colorCounts = {}
local minX, maxX, minY, maxY = nil, nil, nil, nil

for y = 0, H - 1 do
  for x = 0, W - 1 do
    local exported = artLayer:cel(1).image:getPixel(x, y)
    local approved = approvedImg:getPixel(x, y)
    if exported ~= approved then mismatches = mismatches + 1 end

    local a = app.pixelColor.rgbaA(exported)
    if a == 255 then
      visibleCount = visibleCount + 1
      local hex = string.format("#%02X%02X%02X",
        app.pixelColor.rgbaR(exported), app.pixelColor.rgbaG(exported), app.pixelColor.rgbaB(exported))
      colorCounts[hex] = (colorCounts[hex] or 0) + 1
      if not PALETTE_HEX[hex] then outOfPaletteCount = outOfPaletteCount + 1 end
      if minX == nil or x < minX then minX = x end
      if maxX == nil or x > maxX then maxX = x end
      if minY == nil or y < minY then minY = y end
      if maxY == nil or y > maxY then maxY = y end
    elseif a > 0 then
      partialCount = partialCount + 1
    end
  end
end

print("")
print("--- Production export audit ---")
print(string.format("pixel-identical to approved B2-A source: %s (mismatches: %d)",
  (mismatches == 0) and "YES" or "NO", mismatches))
print(string.format("dimensions: %dx%d", W, H))
print("image mode: RGBA")
local sortedHex = {}
for hex, _ in pairs(colorCounts) do table.insert(sortedHex, hex) end
table.sort(sortedHex)
for _, hex in ipairs(sortedHex) do
  print(string.format("  %s : %d px (%.1f%%)", hex, colorCounts[hex], 100 * colorCounts[hex] / visibleCount))
end
print(string.format("visible pixels: %d, partial-alpha: %d, out-of-palette: %d", visibleCount, partialCount, outOfPaletteCount))
print(string.format("occupied bounding box: x[%d..%d] y[%d..%d]", minX, maxX, minY, maxY))
print(string.format("baseline row: %d", maxY))

spr:close()
print("")
print("Done.")
