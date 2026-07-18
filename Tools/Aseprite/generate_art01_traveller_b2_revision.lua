--[[
ART-01 -- Static Traveller, B2 revision (B2-A Balanced / B2-B Fragile).

Supersedes the rejected A/B/C round (see generate_art01_traveller_candidates.lua
for that history). Fixes from the art-direction review:
  - shoulders, sleeves, and a hood opening added (no more pawn/monument read)
  - the flat full-width Bone hem bar removed in favor of a stepped hem
  - palette dominance inverted: Bone is now the primary silhouette color

Hand-authored pixel data as explicit per-row color spans (not AI-generated,
not traced). Produces:
  - Assets/Art/Source/Aseprite/chr_traveller_base_revision_b2.aseprite
  - Assets/Art/Review/ART-01/chr_traveller_b2_a_balanced_24x24.png
  - Assets/Art/Review/ART-01/chr_traveller_b2_b_fragile_24x24.png
  - Assets/Art/Review/ART-01/chr_traveller_b2_contactsheet_8x.png
and prints a technical audit for each candidate to stdout.

Usage:
  "$ASEPRITE_BIN" -b --script Tools/Aseprite/generate_art01_traveller_b2_revision.lua

Self-locates the repository root from its own file path -- no dependency on
caller working directory, no hardcoded developer home path.
Deterministic: rerunning reproduces byte-identical output. Existing files
are reported before being overwritten.
--]]

----------------------------------------------------------------------
-- Repo-root self-location
----------------------------------------------------------------------

local function scriptDir()
  local src = debug.getinfo(1, "S").source
  local path = src:match("^@(.*)$") or src
  return path:match("^(.*)[/\\][^/\\]+$")
end

local projectRoot = scriptDir() .. "/../.."
local sourceOutDir = projectRoot .. "/Assets/Art/Source/Aseprite"
local reviewOutDir = projectRoot .. "/Assets/Art/Review/ART-01"

os.execute(string.format("mkdir -p '%s'", sourceOutDir))
os.execute(string.format("mkdir -p '%s'", reviewOutDir))

local sourcePath = sourceOutDir .. "/chr_traveller_base_revision_b2.aseprite"
local pngPathA = reviewOutDir .. "/chr_traveller_b2_a_balanced_24x24.png"
local pngPathB = reviewOutDir .. "/chr_traveller_b2_b_fragile_24x24.png"
local contactSheetPath = reviewOutDir .. "/chr_traveller_b2_contactsheet_8x.png"

local function checkOverwrite(path)
  if app.fs.isFile(path) then
    print("NOTICE: overwriting existing file: " .. path)
  end
end

----------------------------------------------------------------------
-- Palette
----------------------------------------------------------------------

local W, H = 24, 24

local BONE  = app.pixelColor.rgba(217, 225, 232, 255) -- #D9E1E8
local STONE = app.pixelColor.rgba(42, 49, 66, 255)    -- #2A3142
local NAVY  = app.pixelColor.rgba(18, 27, 45, 255)    -- #121B2D
local CLEAR = app.pixelColor.rgba(0, 0, 0, 0)

-- Contact-sheet-only test marker. Uses the project's established Relic Gold
-- (#F6C85F, Technical Art Baseline PDF sec.8). Never written into the
-- candidate layers/images, only composited into the sheet image directly.
local RELIC_GOLD_TEST = app.pixelColor.rgba(246, 200, 95, 255)

local GUIDE_BASELINE = app.pixelColor.rgba(255, 0, 255, 255)
local GUIDE_CENTER   = app.pixelColor.rgba(0, 255, 255, 255)

-- Reserved chest area, identical for both candidates so a future relic
-- child sits consistently regardless of which candidate is approved.
local CHEST_X0, CHEST_X1 = 10, 13
local CHEST_Y0, CHEST_Y1 = 9, 12

----------------------------------------------------------------------
-- Candidate silhouettes: explicit per-row spans {x0, x1, color}.
--
-- Shared skeleton: hood (y2-y6, with a Navy hood-opening void), neckline
-- shadow (y7), shoulders (y7), sleeve-shadow flanks converging into a
-- clean 4x4 chest reserve (y9-y12, cols 10-13 on both candidates), torso
-- (y13), robe flare with Stone lower-shadow flanks (y14-y20), and a
-- stepped 3-tab Bone hem (y21) instead of a flat pedestal bar.
----------------------------------------------------------------------

local ROWS_B2A = { -- Balanced: symmetric, shoulders 10px
  {y=2,  spans={{10,13,BONE}}},
  {y=3,  spans={{9,14,BONE}}},
  {y=4,  spans={{8,15,BONE}}},
  {y=5,  spans={{8,9,BONE},{10,13,NAVY},{14,15,BONE}}},
  {y=6,  spans={{8,9,BONE},{10,13,NAVY},{14,15,BONE}}},
  {y=7,  spans={{7,10,BONE},{11,12,NAVY},{13,16,BONE}}},
  {y=8,  spans={{7,7,STONE},{8,15,BONE},{16,16,STONE}}},
  {y=9,  spans={{8,9,STONE},{10,13,BONE},{14,15,STONE}}},
  {y=10, spans={{8,9,STONE},{10,13,BONE},{14,15,STONE}}},
  {y=11, spans={{8,9,STONE},{10,13,BONE},{14,15,STONE}}},
  {y=12, spans={{8,9,STONE},{10,13,BONE},{14,15,STONE}}},
  {y=13, spans={{8,15,BONE}}},
  {y=14, spans={{7,16,BONE}}},
  {y=15, spans={{7,7,STONE},{8,15,BONE},{16,16,STONE}}},
  {y=16, spans={{6,7,STONE},{8,15,BONE},{16,17,STONE}}},
  {y=17, spans={{6,7,STONE},{8,15,BONE},{16,17,STONE}}},
  {y=18, spans={{5,6,STONE},{7,16,BONE},{17,18,STONE}}},
  {y=19, spans={{5,6,STONE},{7,16,BONE},{17,18,STONE}}},
  {y=20, spans={{5,6,STONE},{7,16,BONE},{17,18,STONE}}},
  {y=21, spans={{6,8,BONE},{11,13,BONE},{15,17,BONE}}}, -- 3 symmetric hem tabs
}

local ROWS_B2B = { -- Fragile: shoulders 9px, one asymmetric robe step (y17)
  {y=2,  spans={{10,12,BONE}}},
  {y=3,  spans={{9,13,BONE}}},
  {y=4,  spans={{8,14,BONE}}},
  {y=5,  spans={{8,9,BONE},{10,12,NAVY},{13,14,BONE}}},
  {y=6,  spans={{8,9,BONE},{10,12,NAVY},{13,14,BONE}}},
  {y=7,  spans={{7,9,BONE},{10,12,NAVY},{13,15,BONE}}},
  {y=8,  spans={{7,7,STONE},{8,14,BONE},{15,15,STONE}}},
  {y=9,  spans={{8,9,STONE},{10,13,BONE},{14,15,STONE}}},
  {y=10, spans={{8,9,STONE},{10,13,BONE},{14,15,STONE}}},
  {y=11, spans={{8,9,STONE},{10,13,BONE},{14,15,STONE}}},
  {y=12, spans={{8,9,STONE},{10,13,BONE},{14,15,STONE}}},
  {y=13, spans={{8,15,BONE}}},
  {y=14, spans={{8,16,BONE}}},
  {y=15, spans={{8,8,STONE},{9,15,BONE},{16,16,STONE}}},
  {y=16, spans={{7,8,STONE},{9,15,BONE},{16,17,STONE}}},
  {y=17, spans={{5,7,STONE},{8,16,BONE},{17,17,STONE}}}, -- asymmetric bump: left reaches col5, right only col17
  {y=18, spans={{6,7,STONE},{8,16,BONE},{17,18,STONE}}},
  {y=19, spans={{5,6,STONE},{7,16,BONE},{17,18,STONE}}},
  {y=20, spans={{5,6,STONE},{7,16,BONE},{17,18,STONE}}},
  {y=21, spans={{6,8,BONE},{11,13,BONE},{16,17,BONE}}}, -- 3 tabs, uneven sizes (asymmetric)
}

local function buildImage(rows)
  local img = Image(W, H, ColorMode.RGB)
  for y = 0, H - 1 do
    for x = 0, W - 1 do
      img:drawPixel(x, y, CLEAR)
    end
  end
  for _, row in ipairs(rows) do
    for _, span in ipairs(row.spans) do
      local x0, x1, color = span[1], span[2], span[3]
      for x = x0, x1 do
        img:drawPixel(x, row.y, color)
      end
    end
  end
  return img
end

local imgB2A = buildImage(ROWS_B2A)
local imgB2B = buildImage(ROWS_B2B)

----------------------------------------------------------------------
-- Sprite / layer assembly
----------------------------------------------------------------------

local spr = Sprite(W, H, ColorMode.RGB)
spr.filename = sourcePath

local pal = Palette(4)
pal:setColor(0, Color{r=0, g=0, b=0, a=0})
pal:setColor(1, Color{r=217, g=225, b=232, a=255})
pal:setColor(2, Color{r=42, g=49, b=66, a=255})
pal:setColor(3, Color{r=18, g=27, b=45, a=255})
spr:setPalette(pal)

local function setLayerImage(layer, img)
  local cel = layer:cel(1)
  if cel == nil then cel = spr:newCel(layer, 1) end
  cel.image = img
end

local layerA = spr.layers[1]
layerA.name = "B2_A_Balanced"
setLayerImage(layerA, imgB2A)

local layerB = spr:newLayer()
layerB.name = "B2_B_Fragile"
setLayerImage(layerB, imgB2B)

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
setLayerImage(guidesLayer, guidesImg)
guidesLayer.isVisible = false

layerA.isVisible = true
layerB.isVisible = false

checkOverwrite(sourcePath)
spr:saveAs(sourcePath)
print("Saved source: " .. sourcePath)

----------------------------------------------------------------------
-- Individual review exports (clean images, no relic marker, transparent bg)
----------------------------------------------------------------------

local exportTargets = { {layer = layerA, path = pngPathA}, {layer = layerB, path = pngPathB} }
for _, target in ipairs(exportTargets) do
  layerA.isVisible = (target.layer == layerA)
  layerB.isVisible = (target.layer == layerB)
  guidesLayer.isVisible = false
  checkOverwrite(target.path)
  spr:saveCopyAs(target.path)
  print("Exported: " .. target.path)
end
layerA.isVisible = true
layerB.isVisible = false

----------------------------------------------------------------------
-- Technical audit
----------------------------------------------------------------------

local PALETTE_HEX = { ["#D9E1E8"] = true, ["#2A3142"] = true, ["#121B2D"] = true }

local function auditImage(img, label)
  local w, h = img.width, img.height
  local visibleCount, partialCount, outOfPaletteCount = 0, 0, 0
  local colorCounts = {}
  local minX, maxX, minY, maxY = nil, nil, nil, nil
  local occupied = {}

  for y = 0, h - 1 do
    occupied[y] = {}
    for x = 0, w - 1 do
      local c = img:getPixel(x, y)
      local a = app.pixelColor.rgbaA(c)
      if a == 255 then
        visibleCount = visibleCount + 1
        local hex = string.format("#%02X%02X%02X",
          app.pixelColor.rgbaR(c), app.pixelColor.rgbaG(c), app.pixelColor.rgbaB(c))
        colorCounts[hex] = (colorCounts[hex] or 0) + 1
        if not PALETTE_HEX[hex] then outOfPaletteCount = outOfPaletteCount + 1 end
        if minX == nil or x < minX then minX = x end
        if maxX == nil or x > maxX then maxX = x end
        if minY == nil or y < minY then minY = y end
        if maxY == nil or y > maxY then maxY = y end
        occupied[y][x] = true
      elseif a > 0 then
        partialCount = partialCount + 1
        occupied[y][x] = true
      else
        occupied[y][x] = false
      end
    end
  end

  local visited = {}
  for y = 0, h - 1 do
    visited[y] = {}
    for x = 0, w - 1 do visited[y][x] = false end
  end
  local componentSizes = {}
  for y = 0, h - 1 do
    for x = 0, w - 1 do
      if occupied[y][x] and not visited[y][x] then
        local stack = {{x, y}}
        visited[y][x] = true
        local size = 0
        while #stack > 0 do
          local cur = table.remove(stack)
          size = size + 1
          local cx, cy = cur[1], cur[2]
          local neigh = {{cx+1,cy},{cx-1,cy},{cx,cy+1},{cx,cy-1}}
          for _, n in ipairs(neigh) do
            local nx, ny = n[1], n[2]
            if nx >= 0 and nx < w and ny >= 0 and ny < h
               and occupied[ny][nx] and not visited[ny][nx] then
              visited[ny][nx] = true
              table.insert(stack, {nx, ny})
            end
          end
        end
        table.insert(componentSizes, size)
      end
    end
  end
  local isolatedSingles = 0
  for _, s in ipairs(componentSizes) do
    if s == 1 then isolatedSingles = isolatedSingles + 1 end
  end

  local topPad = minY or h
  local bottomPad = (maxY == nil) and h or (h - 1 - maxY)
  local leftPad = minX or w
  local rightPad = (maxX == nil) and w or (w - 1 - maxX)
  local canvasCenterX = (w - 1) / 2
  local bboxCenterX = (minX == nil) and 0 or ((minX + maxX) / 2)

  -- Chest reserve cleanliness: must be fully Bone, no other color, no gaps.
  local chestClean = true
  local chestPixelCount = 0
  for y = CHEST_Y0, CHEST_Y1 do
    for x = CHEST_X0, CHEST_X1 do
      local c = img:getPixel(x, y)
      chestPixelCount = chestPixelCount + 1
      if not (app.pixelColor.rgbaA(c) == 255
        and app.pixelColor.rgbaR(c) == 217
        and app.pixelColor.rgbaG(c) == 225
        and app.pixelColor.rgbaB(c) == 232) then
        chestClean = false
      end
    end
  end

  local sortedHex = {}
  for hex, _ in pairs(colorCounts) do table.insert(sortedHex, hex) end
  table.sort(sortedHex)

  print(string.format("--- Technical audit: %s ---", label))
  print(string.format("dimensions: %dx%d", w, h))
  print("image mode: RGBA")
  print("colors:")
  for _, hex in ipairs(sortedHex) do
    local pct = 100 * colorCounts[hex] / visibleCount
    print(string.format("  %s : %d px (%.1f%%)%s", hex, colorCounts[hex], pct,
      PALETTE_HEX[hex] and "" or "  [OUT OF PALETTE]"))
  end
  print(string.format("visible pixels (alpha=255): %d", visibleCount))
  print(string.format("partial-alpha pixels: %d", partialCount))
  print(string.format("out-of-palette pixel count: %d", outOfPaletteCount))
  print(string.format("occupied bounding box: x[%d..%d] y[%d..%d] (%dx%d)",
    minX, maxX, minY, maxY, (maxX - minX + 1), (maxY - minY + 1)))
  print(string.format("padding: top=%d bottom=%d left=%d right=%d", topPad, bottomPad, leftPad, rightPad))
  print(string.format("top visible row: %d (must be <= 2)", minY))
  print(string.format("center offset from canvas center: %.2f px (canvas cx=%.1f, bbox cx=%.1f)",
    bboxCenterX - canvasCenterX, canvasCenterX, bboxCenterX))
  print(string.format("baseline row (bbox bottom, max Y): %d (must == 21)", maxY))
  print(string.format("connected components: %d", #componentSizes))
  print(string.format("isolated single-pixel clusters: %d", isolatedSingles))
  print(string.format("chest reserve [%d..%d]x[%d..%d]: %d px, all-Bone clean = %s",
    CHEST_X0, CHEST_X1, CHEST_Y0, CHEST_Y1, chestPixelCount, tostring(chestClean)))
  print("")

  return {
    label = label, visibleCount = visibleCount, minX = minX, maxX = maxX,
    minY = minY, maxY = maxY, isolatedSingles = isolatedSingles, chestClean = chestClean,
  }
end

print("")
local auditA = auditImage(imgB2A, "B2-A Balanced")
local auditB = auditImage(imgB2B, "B2-B Fragile")

print(string.format("Baseline consistency: A.maxY=%d B.maxY=%d -> %s",
  auditA.maxY, auditB.maxY, (auditA.maxY == auditB.maxY) and "CONSISTENT" or "MISMATCH"))
print(string.format("Top-row bound check: A.minY=%d B.minY=%d -> %s",
  auditA.minY, auditB.minY,
  (auditA.minY <= 2 and auditB.minY <= 2) and "PASS (both <= 2)" or "FAIL"))
print(string.format("Occupied width: A=%d B=%d (target 12-14)", auditA.maxX-auditA.minX+1, auditB.maxX-auditB.minX+1))
print(string.format("Occupied height: A=%d B=%d (target 18-20)", auditA.maxY-auditA.minY+1, auditB.maxY-auditB.minY+1))
print("")

----------------------------------------------------------------------
-- Contact sheet: nearest-neighbour 8x, Deep Navy backing, A/B labeled
-- outside canvases, temporary gold relic marker over the chest reserve
-- (sheet-compositing only -- never touches imgB2A/imgB2B or the exported
-- candidate PNGs, which were already written above from the clean images).
----------------------------------------------------------------------

local scale = 8
local cellW, cellH = W * scale, H * scale
local margin, gap, labelGap, labelH = 8, 24, 8, 20
local sheetW = margin * 2 + cellW * 2 + gap
local sheetH = margin * 2 + cellH + labelGap + labelH

local sheetImg = Image(sheetW, sheetH, ColorMode.RGB)
for y = 0, sheetH - 1 do
  for x = 0, sheetW - 1 do
    sheetImg:drawPixel(x, y, NAVY)
  end
end

local function blitNearestNeighbor8x(srcImg, destImg, destX, destY, s)
  for y = 0, srcImg.height - 1 do
    for x = 0, srcImg.width - 1 do
      local c = srcImg:getPixel(x, y)
      if app.pixelColor.rgbaA(c) > 0 then
        for dy = 0, s - 1 do
          for dx = 0, s - 1 do
            destImg:drawPixel(destX + x * s + dx, destY + y * s + dy, c)
          end
        end
      end
    end
  end
end

local GLYPHS = {
  A = { " XXX ", "X...X", "X...X", "XXXXX", "X...X", "X...X", "X...X" },
  B = { "XXXX ", "X...X", "X...X", "XXXX ", "X...X", "X...X", "XXXX " },
}

local function stampGlyph(destImg, letter, destX, destY, glyphScale, color)
  local glyph = GLYPHS[letter]
  for gy = 1, #glyph do
    local row = glyph[gy]
    for gx = 1, #row do
      if row:sub(gx, gx) == "X" then
        for dy = 0, glyphScale - 1 do
          for dx = 0, glyphScale - 1 do
            destImg:drawPixel(destX + (gx - 1) * glyphScale + dx, destY + (gy - 1) * glyphScale + dy, color)
          end
        end
      end
    end
  end
end

local candidates = { {name = "A", img = imgB2A}, {name = "B", img = imgB2B} }
for i, cand in ipairs(candidates) do
  local cellX = margin + (i - 1) * (cellW + gap)
  local cellY = margin
  blitNearestNeighbor8x(cand.img, sheetImg, cellX, cellY, scale)

  -- Temporary gold relic marker over the reserved chest area, sheet-only.
  for y = CHEST_Y0, CHEST_Y1 do
    for x = CHEST_X0, CHEST_X1 do
      for dy = 0, scale - 1 do
        for dx = 0, scale - 1 do
          sheetImg:drawPixel(cellX + x * scale + dx, cellY + y * scale + dy, RELIC_GOLD_TEST)
        end
      end
    end
  end

  local glyphScale = 2
  local glyphW = 5 * glyphScale
  local labelX = cellX + math.floor((cellW - glyphW) / 2)
  local labelY = cellY + cellH + labelGap
  stampGlyph(sheetImg, cand.name, labelX, labelY, glyphScale, BONE)
end

local sheetSpr = Sprite(sheetW, sheetH, ColorMode.RGB)
local sheetLayer = sheetSpr.layers[1]
local sheetCel = sheetLayer:cel(1)
if sheetCel == nil then sheetCel = sheetSpr:newCel(sheetLayer, 1) end
sheetCel.image = sheetImg

checkOverwrite(contactSheetPath)
sheetSpr:saveCopyAs(contactSheetPath)
print("Exported: " .. contactSheetPath)

sheetSpr:close()
spr:close()

print("")
print("Done.")
