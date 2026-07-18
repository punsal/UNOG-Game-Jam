--[[
ART-01 -- Static Traveller candidate generator + technical audit + review contact sheet.

Purpose:
  Hand-authored (not AI-generated, not traced) pixel data for three 24x24
  traveller silhouette candidates, built deterministically from explicit
  per-row width tables defined in this script. Produces:
    - Assets/Art/Source/Aseprite/chr_traveller_base_candidates.aseprite
    - Assets/Art/Review/ART-01/chr_traveller_candidate_{a,b,c}_24x24.png
    - Assets/Art/Review/ART-01/chr_traveller_candidates_contactsheet_8x.png
  and prints a technical audit for each candidate to stdout.

Usage:
  Run from the verified Aseprite CLI in batch mode, e.g.:
    "$ASEPRITE_BIN" -b --script Tools/Aseprite/generate_art01_traveller_candidates.lua

  This script self-locates the repository root from its own file path, so it
  does not depend on the caller's working directory and never hardcodes any
  developer's home directory.

Determinism:
  All pixel data is fixed literal tables. Rerunning produces byte-identical
  candidate art every time. Existing output files are reported (not silently
  clobbered) before being overwritten, so reruns are safe.
--]]

----------------------------------------------------------------------
-- Repo-root self-location (no hardcoded absolute paths in this file)
----------------------------------------------------------------------

local function scriptDir()
  local src = debug.getinfo(1, "S").source
  local path = src:match("^@(.*)$") or src
  return path:match("^(.*)[/\\][^/\\]+$")
end

local thisDir = scriptDir() -- .../Tools/Aseprite
local projectRoot = thisDir .. "/../.."

local sourceOutDir = projectRoot .. "/Assets/Art/Source/Aseprite"
local reviewOutDir = projectRoot .. "/Assets/Art/Review/ART-01"

os.execute(string.format("mkdir -p '%s'", sourceOutDir))
os.execute(string.format("mkdir -p '%s'", reviewOutDir))

local sourcePath = sourceOutDir .. "/chr_traveller_base_candidates.aseprite"
local pngPathA = reviewOutDir .. "/chr_traveller_candidate_a_24x24.png"
local pngPathB = reviewOutDir .. "/chr_traveller_candidate_b_24x24.png"
local pngPathC = reviewOutDir .. "/chr_traveller_candidate_c_24x24.png"
local contactSheetPath = reviewOutDir .. "/chr_traveller_candidates_contactsheet_8x.png"

local function checkOverwrite(path)
  if app.fs.isFile(path) then
    print("NOTICE: overwriting existing file: " .. path)
  end
end

----------------------------------------------------------------------
-- Approved palette (Assets/Art/TECHNICAL_ART_DECISIONS.md / PDF section 8)
----------------------------------------------------------------------

local W, H = 24, 24

local BONE  = app.pixelColor.rgba(217, 225, 232, 255) -- #D9E1E8
local STONE = app.pixelColor.rgba(42, 49, 66, 255)    -- #2A3142
local NAVY  = app.pixelColor.rgba(18, 27, 45, 255)    -- #121B2D
local CLEAR = app.pixelColor.rgba(0, 0, 0, 0)

-- Guide-only colors. Never used in candidate art, never exported (Guides
-- layer stays hidden and is excluded from every PNG export).
local GUIDE_BASELINE = app.pixelColor.rgba(255, 0, 255, 255)
local GUIDE_CENTER   = app.pixelColor.rgba(0, 255, 255, 255)

----------------------------------------------------------------------
-- Candidate silhouettes.
--
-- Each row is {y, halfWidth, color}. Filled columns for a row are
-- [12-halfWidth, 11+halfWidth], i.e. symmetric around the boundary between
-- columns 11 and 12 (the canvas center for a 24px-wide sprite). Using the
-- same center and the same y1..y21 vertical range for all three candidates
-- guarantees a shared bottom-center baseline and compatible proportions;
-- only per-row mass (halfWidth) differs between candidates.
--
-- Row plan (shared):
--   y1-y6   hood dome (Stone)
--   y7      hood/body shadow seam (Deep Navy) -- separates hood from body
--   y8-y20  robe body, gradually widening toward the hem (Stone)
--   y21     hem baseline row (Bone trim, reinforces the baseline read)
----------------------------------------------------------------------

local ROWS_A = { -- narrow and fragile
  {1,1,STONE},{2,2,STONE},{3,3,STONE},{4,3,STONE},{5,3,STONE},{6,3,STONE},
  {7,3,NAVY},
  {8,2,STONE},{9,2,STONE},{10,2,STONE},
  {11,3,STONE},{12,3,STONE},{13,3,STONE},
  {14,4,STONE},{15,4,STONE},{16,4,STONE},
  {17,5,STONE},{18,5,STONE},{19,5,STONE},
  {20,6,STONE},
  {21,6,BONE},
}

local ROWS_B = { -- balanced and neutral
  {1,1,STONE},{2,2,STONE},{3,3,STONE},{4,4,STONE},{5,4,STONE},{6,4,STONE},
  {7,4,NAVY},
  {8,3,STONE},{9,3,STONE},{10,3,STONE},
  {11,4,STONE},{12,4,STONE},{13,4,STONE},
  {14,5,STONE},{15,5,STONE},{16,5,STONE},
  {17,6,STONE},{18,6,STONE},{19,6,STONE},
  {20,7,STONE},
  {21,7,BONE},
}

local ROWS_C = { -- slightly broader and more stable
  {1,2,STONE},{2,3,STONE},{3,4,STONE},{4,5,STONE},{5,5,STONE},{6,5,STONE},
  {7,5,NAVY},
  {8,4,STONE},{9,4,STONE},{10,4,STONE},
  {11,5,STONE},{12,5,STONE},{13,6,STONE},
  {14,6,STONE},{15,6,STONE},{16,7,STONE},
  {17,7,STONE},{18,7,STONE},{19,8,STONE},
  {20,8,STONE},
  {21,8,BONE},
}

local function buildCandidateImage(rows)
  local img = Image(W, H, ColorMode.RGB)
  for y = 0, H - 1 do
    for x = 0, W - 1 do
      img:drawPixel(x, y, CLEAR)
    end
  end
  for _, row in ipairs(rows) do
    local y, halfWidth, color = row[1], row[2], row[3]
    local x0 = 12 - halfWidth
    local x1 = 11 + halfWidth
    for x = x0, x1 do
      img:drawPixel(x, y, color)
    end
  end
  return img
end

local imgA = buildCandidateImage(ROWS_A)
local imgB = buildCandidateImage(ROWS_B)
local imgC = buildCandidateImage(ROWS_C)

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
  if cel == nil then
    cel = spr:newCel(layer, 1)
  end
  cel.image = img
end

local layerA = spr.layers[1]
layerA.name = "Candidate_A"
setLayerImage(layerA, imgA)

local layerB = spr:newLayer()
layerB.name = "Candidate_B"
setLayerImage(layerB, imgB)

local layerC = spr:newLayer()
layerC.name = "Candidate_C"
setLayerImage(layerC, imgC)

-- Guides layer: baseline (horizontal, hem row) + center-line (vertical,
-- canvas center column), disabled by default. Not exported in any PNG.
local guidesLayer = spr:newLayer()
guidesLayer.name = "Guides"
local guidesImg = Image(W, H, ColorMode.RGB)
for y = 0, H - 1 do
  for x = 0, W - 1 do
    guidesImg:drawPixel(x, y, CLEAR)
  end
end
for x = 0, W - 1 do
  guidesImg:drawPixel(x, 21, GUIDE_BASELINE) -- baseline guide: shared hem row
end
for y = 0, H - 1 do
  guidesImg:drawPixel(12, y, GUIDE_CENTER) -- center-line guide
end
setLayerImage(guidesLayer, guidesImg)
guidesLayer.isVisible = false

-- Default view: only Candidate_A visible. All three candidates share the
-- same 24x24 canvas position, so showing them simultaneously would overlap
-- into a single composited mess -- toggle layer visibility in Aseprite to
-- compare candidates, this is standard practice for same-canvas variants.
layerA.isVisible = true
layerB.isVisible = false
layerC.isVisible = false

checkOverwrite(sourcePath)
spr:saveAs(sourcePath)
print("Saved source: " .. sourcePath)

----------------------------------------------------------------------
-- Individual review exports (transparent background, native 24x24)
----------------------------------------------------------------------

local exportTargets = {
  {layer = layerA, path = pngPathA},
  {layer = layerB, path = pngPathB},
  {layer = layerC, path = pngPathC},
}

for _, target in ipairs(exportTargets) do
  layerA.isVisible = (target.layer == layerA)
  layerB.isVisible = (target.layer == layerB)
  layerC.isVisible = (target.layer == layerC)
  guidesLayer.isVisible = false
  checkOverwrite(target.path)
  spr:saveCopyAs(target.path)
  print("Exported: " .. target.path)
end

-- Restore default in-memory view (cosmetic only; source file on disk was
-- already saved above with Candidate_A as the default visible layer).
layerA.isVisible = true
layerB.isVisible = false
layerC.isVisible = false

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
        if not PALETTE_HEX[hex] then
          outOfPaletteCount = outOfPaletteCount + 1
        end
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

  -- 4-connected component analysis (isolated single-pixel cluster check)
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

  local sortedHex = {}
  for hex, _ in pairs(colorCounts) do table.insert(sortedHex, hex) end
  table.sort(sortedHex)

  print(string.format("--- Technical audit: %s ---", label))
  print(string.format("dimensions: %dx%d", w, h))
  print("image mode: RGBA")
  print("colors:")
  for _, hex in ipairs(sortedHex) do
    print(string.format("  %s : %d px%s", hex, colorCounts[hex],
      PALETTE_HEX[hex] and "" or "  [OUT OF PALETTE]"))
  end
  print(string.format("visible pixels (alpha=255): %d", visibleCount))
  print(string.format("partial-alpha pixels: %d", partialCount))
  print(string.format("out-of-palette pixel count: %d", outOfPaletteCount))
  print(string.format("occupied bounding box: x[%d..%d] y[%d..%d] (%dx%d)",
    minX, maxX, minY, maxY, (maxX - minX + 1), (maxY - minY + 1)))
  print(string.format("padding: top=%d bottom=%d left=%d right=%d", topPad, bottomPad, leftPad, rightPad))
  print(string.format("center offset from canvas center: %.2f px (canvas cx=%.1f, bbox cx=%.1f)",
    bboxCenterX - canvasCenterX, canvasCenterX, bboxCenterX))
  print(string.format("baseline row (bbox bottom, max Y): %d", maxY))
  print(string.format("connected components: %d", #componentSizes))
  print(string.format("isolated single-pixel clusters: %d", isolatedSingles))
  print("")

  return {
    label = label, visibleCount = visibleCount, partialCount = partialCount,
    outOfPaletteCount = outOfPaletteCount, minX = minX, maxX = maxX,
    minY = minY, maxY = maxY, topPad = topPad, bottomPad = bottomPad,
    leftPad = leftPad, rightPad = rightPad, isolatedSingles = isolatedSingles,
  }
end

print("")
local auditA = auditImage(imgA, "Candidate A (narrow / fragile)")
local auditB = auditImage(imgB, "Candidate B (balanced / neutral)")
local auditC = auditImage(imgC, "Candidate C (broader / stable)")

print(string.format("Baseline consistency check: A.maxY=%d B.maxY=%d C.maxY=%d -> %s",
  auditA.maxY, auditB.maxY, auditC.maxY,
  (auditA.maxY == auditB.maxY and auditB.maxY == auditC.maxY) and "CONSISTENT" or "MISMATCH"))
print(string.format("Top-row consistency check: A.minY=%d B.minY=%d C.minY=%d -> %s",
  auditA.minY, auditB.minY, auditC.minY,
  (auditA.minY == auditB.minY and auditB.minY == auditC.minY) and "CONSISTENT" or "MISMATCH"))
print("")

----------------------------------------------------------------------
-- Contact sheet (review only -- solid Deep Navy backing, not transparent;
-- individual candidate PNGs above remain fully transparent per spec)
----------------------------------------------------------------------

local scale = 8
local cellW, cellH = W * scale, H * scale
local margin, gap, labelGap, labelH = 8, 16, 8, 20
local sheetW = margin * 2 + cellW * 3 + gap * 2
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

-- Simple hand-authored 5x7 block glyphs, used only for contact-sheet labels
-- (kept inside Aseprite so no external tool is required).
local GLYPHS = {
  A = { " XXX ", "X...X", "X...X", "XXXXX", "X...X", "X...X", "X...X" },
  B = { "XXXX ", "X...X", "X...X", "XXXX ", "X...X", "X...X", "XXXX " },
  C = { " XXXX", "X....", "X....", "X....", "X....", "X....", " XXXX" },
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

local candidates = { {name = "A", img = imgA}, {name = "B", img = imgB}, {name = "C", img = imgC} }
for i, cand in ipairs(candidates) do
  local cellX = margin + (i - 1) * (cellW + gap)
  local cellY = margin
  blitNearestNeighbor8x(cand.img, sheetImg, cellX, cellY, scale)

  local glyphScale = 2
  local glyphW, glyphH = 5 * glyphScale, 7 * glyphScale
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
