--[[
ART-05 -- Altar pair normalization.

Takes the raw PixelLab reference (Assets/Art/Generated/References/ART-05/
altar_safe_RAW.png, 64x96) as shape reference only. Downscales 2x by point
sampling (keeps crisp pixel-art edges, no blending), quantizes every opaque
pixel to the nearest approved body color (Void/Deep Navy/Stone), forces hard
alpha (0/255), then stamps a hand-placed, phone-readable inset gem shape on
top -- identical position/size on both altars, only the inset color differs
(Cyan for safe, Gold for cost). This guarantees identical outer mass between
the pair per the Technical Art Baseline ("share the same outer mass...
differentiate through inset").

The cost altar's own raw reference is not pixel-sampled -- reusing the safe
altar's quantized body for both is what makes the pair *exactly* identical,
which independent quantization of two separate AI generations cannot
guarantee.
--]]

local function scriptDir()
  local src = debug.getinfo(1, "S").source
  local path = src:match("^@(.*)$") or src
  return path:match("^(.*)[/\\][^/\\]+$")
end
local root = scriptDir() .. "/../.."
local refPath = root .. "/Assets/Art/Generated/References/ART-05/altar_safe_RAW.png"
local srcDir = root .. "/Assets/Art/Source/Aseprite"
local prodDir = root .. "/Assets/Art/Production/Altars"
os.execute(string.format("mkdir -p '%s'", srcDir))
os.execute(string.format("mkdir -p '%s'", prodDir))

local function checkOverwrite(path)
  if app.fs.isFile(path) then print("NOTICE: overwriting existing file: " .. path) end
end

local VOID  = app.pixelColor.rgba(10, 13, 20, 255)
local NAVY  = app.pixelColor.rgba(18, 27, 45, 255)
local STONE = app.pixelColor.rgba(42, 49, 66, 255)
local CYAN  = app.pixelColor.rgba(95, 169, 196, 255)
local GOLD  = app.pixelColor.rgba(246, 200, 95, 255)
local CLEAR = app.pixelColor.rgba(0, 0, 0, 0)

local BODY_PALETTE = {
  {10, 13, 20, VOID}, {18, 27, 45, NAVY}, {42, 49, 66, STONE},
}

local function nearestBodyColor(r, g, b)
  local best, bestDist = STONE, math.huge
  for _, c in ipairs(BODY_PALETTE) do
    local dr, dg, db = r - c[1], g - c[2], b - c[3]
    local dist = dr*dr + dg*dg + db*db
    if dist < bestDist then bestDist = dist; best = c[4] end
  end
  return best
end

if not app.fs.isFile(refPath) then
  error("Reference not found: " .. refPath .. " (run the PixelLab generation step first)")
end

local rawSpr = app.open(refPath)
local rawImg = rawSpr.layers[1]:cel(1).image -- 64x96, raw AI output

local W, H = 32, 48
local bodyImg = Image(W, H, ColorMode.RGB)
for y = 0, H - 1 do
  for x = 0, W - 1 do
    local rx, ry = x * 2, y * 2 -- point-sample, no blending
    local c = rawImg:getPixel(rx, ry)
    local a = app.pixelColor.rgbaA(c)
    if a >= 128 then
      bodyImg:drawPixel(x, y, nearestBodyColor(app.pixelColor.rgbaR(c), app.pixelColor.rgbaG(c), app.pixelColor.rgbaB(c)))
    else
      bodyImg:drawPixel(x, y, CLEAR)
    end
  end
end
rawSpr:close() -- reference untouched on disk

-- Hand-placed, phone-readable inset gem (identical on both altars, color only
-- differs). Rounded diamond, 6 wide x 8 tall, centered horizontally, sitting
-- in the upper-middle of the altar body where the AI reference placed its gem.
local insetCx, insetCy = 15.5, 17 -- canvas-relative center
local insetSpans = {
  {y=13, dx={-1,1}}, {y=14, dx={-2,2}}, {y=15, dx={-3,3}}, {y=16, dx={-3,3}},
  {y=17, dx={-3,3}}, {y=18, dx={-3,3}}, {y=19, dx={-3,3}}, {y=20, dx={-2,2}},
  {y=21, dx={-1,1}},
}

local function stampInset(img, color)
  for _, row in ipairs(insetSpans) do
    for dx = row.dx[1], row.dx[2] do
      local x = math.floor(insetCx) + dx
      if x >= 0 and x < W and row.y >= 0 and row.y < H then
        img:drawPixel(x, row.y, color)
      end
    end
  end
end

local safeImg = bodyImg:clone()
stampInset(safeImg, CYAN)

local costImg = bodyImg:clone()
stampInset(costImg, GOLD)

----------------------------------------------------------------------
-- Save source (two layers) + production exports
----------------------------------------------------------------------

local sourcePath = srcDir .. "/altar_pair.aseprite"
local spr = Sprite(W, H, ColorMode.RGB)
spr.filename = sourcePath
local safeLayer = spr.layers[1]
safeLayer.name = "Altar_Safe"
local safeCel = safeLayer:cel(1)
if safeCel == nil then safeCel = spr:newCel(safeLayer, 1) end
safeCel.image = safeImg

local costLayer = spr:newLayer()
costLayer.name = "Altar_Cost"
local costCel = spr:newCel(costLayer, 1)
costCel.image = costImg

safeLayer.isVisible = true
costLayer.isVisible = false
checkOverwrite(sourcePath)
spr:saveAs(sourcePath)
print("Saved source: " .. sourcePath)

local safePngPath = prodDir .. "/altar_safe_idle.png"
local costPngPath = prodDir .. "/altar_cost_idle.png"

safeLayer.isVisible = true
costLayer.isVisible = false
checkOverwrite(safePngPath)
spr:saveCopyAs(safePngPath)
print("Exported: " .. safePngPath)

safeLayer.isVisible = false
costLayer.isVisible = true
checkOverwrite(costPngPath)
spr:saveCopyAs(costPngPath)
print("Exported: " .. costPngPath)

safeLayer.isVisible = true
costLayer.isVisible = false
spr:close()

----------------------------------------------------------------------
-- Fast checks
----------------------------------------------------------------------

local mismatches = 0
for y = 0, H - 1 do
  for x = 0, W - 1 do
    local sc, cc = safeImg:getPixel(x, y), costImg:getPixel(x, y)
    local sIsInset = (sc == CYAN)
    local cIsInset = (cc == GOLD)
    if sIsInset ~= cIsInset then mismatches = mismatches + 1
    elseif not sIsInset and sc ~= cc then mismatches = mismatches + 1 end
  end
end
print("")
print(string.format("Outer-mass identity check (body pixels + inset footprint must match): mismatches=%d -> %s",
  mismatches, mismatches == 0 and "IDENTICAL" or "MISMATCH"))
print("Done.")
