--[[
Batch 1 -- deterministic Aseprite assets for the P0 pack (jam-speed pass).
ART-02 Relic, ART-03 Spike module, ART-04 Moving blade, ART-07 Corridor kit.

Hand-authored pixel data (explicit spans / simple closed-form shape rules),
no AI generation. Self-locates repo root, no hardcoded home path.
Deterministic and safe to rerun; reports before overwriting.
--]]

local function scriptDir()
  local src = debug.getinfo(1, "S").source
  local path = src:match("^@(.*)$") or src
  return path:match("^(.*)[/\\][^/\\]+$")
end
local root = scriptDir() .. "/../.."
local srcDir = root .. "/Assets/Art/Source/Aseprite"
local vfxDir = root .. "/Assets/Art/Production/VFX"
local hazDir = root .. "/Assets/Art/Production/Hazards"
local envDir = root .. "/Assets/Art/Production/Environment"
for _, d in ipairs({srcDir, vfxDir, hazDir, envDir}) do
  os.execute(string.format("mkdir -p '%s'", d))
end

local function checkOverwrite(path)
  if app.fs.isFile(path) then print("NOTICE: overwriting existing file: " .. path) end
end

local GOLD  = app.pixelColor.rgba(246, 200, 95, 255)
local STONE = app.pixelColor.rgba(42, 49, 66, 255)
local NAVY  = app.pixelColor.rgba(18, 27, 45, 255)
local VOID  = app.pixelColor.rgba(10, 13, 20, 255)
local RED   = app.pixelColor.rgba(217, 74, 90, 255)
local PURPLE= app.pixelColor.rgba(117, 89, 142, 255)
local CLEAR = app.pixelColor.rgba(0, 0, 0, 0)

local function blank(w, h)
  local img = Image(w, h, ColorMode.RGB)
  for y = 0, h - 1 do
    for x = 0, w - 1 do img:drawPixel(x, y, CLEAR) end
  end
  return img
end

local function fillSpans(img, rows)
  for _, row in ipairs(rows) do
    for _, span in ipairs(row.spans) do
      for x = span[1], span[2] do img:drawPixel(x, row.y, span[3]) end
    end
  end
end

-- Builds a single-layer sprite, saves source .aseprite, exports flattened PNG.
local function buildAndExport(w, h, layerName, img, sourcePath, pngPath, extraLayerFn)
  local spr = Sprite(w, h, ColorMode.RGB)
  spr.filename = sourcePath
  local layer = spr.layers[1]
  layer.name = layerName
  local cel = layer:cel(1)
  if cel == nil then cel = spr:newCel(layer, 1) end
  cel.image = img
  if extraLayerFn then extraLayerFn(spr) end
  checkOverwrite(sourcePath)
  spr:saveAs(sourcePath)
  print("Saved source: " .. sourcePath)
  checkOverwrite(pngPath)
  spr:saveCopyAs(pngPath)
  print("Exported: " .. pngPath)
  spr:close()
end

----------------------------------------------------------------------
-- ART-02: fx_relic_core.png (8x8) + fx_relic_glow_inner.png (16x16)
-- Combined into one source file with two layers, two separate exports.
----------------------------------------------------------------------

local coreImg = blank(8, 8)
fillSpans(coreImg, {
  {y=2, spans={{3,4,GOLD}}},
  {y=3, spans={{2,5,GOLD}}},
  {y=4, spans={{2,5,GOLD}}},
  {y=5, spans={{2,5,GOLD}}},
  {y=6, spans={{3,4,GOLD}}},
})

local glowImg = blank(16, 16)
for y = 0, 15 do
  for x = 0, 15 do
    local dist = math.abs(x - 7.5) + math.abs(y - 7.5) -- diamond (Manhattan) distance
    local a = 0
    if dist <= 3.0 then a = 255
    elseif dist <= 5.5 then a = 170
    elseif dist <= 7.5 then a = 85
    end
    if a > 0 then
      glowImg:drawPixel(x, y, app.pixelColor.rgba(246, 200, 95, a))
    end
  end
end

local relicSourcePath = srcDir .. "/fx_relic.aseprite"
local relicSpr = Sprite(16, 16, ColorMode.RGB)
relicSpr.filename = relicSourcePath
local coreLayer = relicSpr.layers[1]
coreLayer.name = "RelicCore"
-- core is 8x8, centered inside a 16x16 working canvas for layer parity, then
-- exported at its own 8x8 crop below.
local coreCelFull = coreLayer:cel(1)
if coreCelFull == nil then coreCelFull = relicSpr:newCel(coreLayer, 1) end
local coreFull16 = blank(16, 16)
for y = 0, 7 do
  for x = 0, 7 do
    local c = coreImg:getPixel(x, y)
    if app.pixelColor.rgbaA(c) > 0 then coreFull16:drawPixel(x + 4, y + 4, c) end
  end
end
coreCelFull.image = coreFull16

local glowLayer = relicSpr:newLayer()
glowLayer.name = "GlowInner"
local glowCel = relicSpr:newCel(glowLayer, 1)
glowCel.image = glowImg
glowLayer.isVisible = true
coreLayer.isVisible = true

checkOverwrite(relicSourcePath)
relicSpr:saveAs(relicSourcePath)
print("Saved source: " .. relicSourcePath)

-- Export glow (16x16, full canvas)
glowLayer.isVisible = true
coreLayer.isVisible = false
local glowPngPath = vfxDir .. "/fx_relic_glow_inner.png"
checkOverwrite(glowPngPath)
relicSpr:saveCopyAs(glowPngPath)
print("Exported: " .. glowPngPath)

relicSpr:close()

-- Export core as its own true 8x8 file (separate sprite, exact canvas)
local corePngPath = vfxDir .. "/fx_relic_core.png"
buildAndExport(8, 8, "RelicCore", coreImg, srcDir .. "/fx_relic_core_STANDALONE_UNUSED.aseprite", corePngPath)
os.remove(srcDir .. "/fx_relic_core_STANDALONE_UNUSED.aseprite") -- source already lives in fx_relic.aseprite above

----------------------------------------------------------------------
-- ART-03: haz_spike_module.png (16x16), horizontally repeatable
----------------------------------------------------------------------

local spikeImg = blank(16, 16)
fillSpans(spikeImg, {
  {y=9,  spans={{4,4,RED},{12,12,RED}}},
  {y=10, spans={{3,5,RED},{11,13,RED}}},
  {y=11, spans={{2,6,STONE},{10,14,STONE}}},
  {y=12, spans={{2,6,STONE},{10,14,STONE}}},
  {y=13, spans={{0,15,STONE}}},
  {y=14, spans={{0,15,STONE}}},
  {y=15, spans={{0,15,STONE}}},
})
buildAndExport(16, 16, "SpikeModule", spikeImg,
  srcDir .. "/haz_spike_module.aseprite", hazDir .. "/haz_spike_module.png")

----------------------------------------------------------------------
-- ART-04: haz_blade_base.png (32x32), plus-cross with tapered tips
----------------------------------------------------------------------

local bladeImg = blank(32, 32)
local cx, cy = 15.5, 15.5
for y = 0, 31 do
  for x = 0, 31 do
    local dx, dy = x - cx, y - cy
    local onH = math.abs(dy) <= 3 and math.abs(dx) <= 13.5
    local onV = math.abs(dx) <= 3 and math.abs(dy) <= 13.5
    if onH or onV then
      -- taper: reduce allowed half-thickness near the tips for a pointed blade look
      local thickness = 3
      if onH then
        local reach = 13.5 - math.abs(dx)
        if reach < 4 then thickness = math.max(0, math.floor(reach * 0.75)) end
        if math.abs(dy) <= thickness then bladeImg:drawPixel(x, y, STONE) end
      end
      if onV then
        local reach = 13.5 - math.abs(dy)
        local thicknessV = 3
        if reach < 4 then thicknessV = math.max(0, math.floor(reach * 0.75)) end
        if math.abs(dx) <= thicknessV then bladeImg:drawPixel(x, y, STONE) end
      end
    end
  end
end
-- central hub, Shadow Purple diamond, drawn on top
for y = 0, 31 do
  for x = 0, 31 do
    local dist = math.abs(x - cx) + math.abs(y - cy)
    if dist <= 3.5 then bladeImg:drawPixel(x, y, PURPLE) end
  end
end
buildAndExport(32, 32, "BladeBase", bladeImg,
  srcDir .. "/haz_blade_base.aseprite", hazDir .. "/haz_blade_base.png")

----------------------------------------------------------------------
-- ART-07: corridor micro-kit (6 files, one shared source)
----------------------------------------------------------------------

local floorClean = blank(16, 16)
for y = 0, 15 do for x = 0, 15 do floorClean:drawPixel(x, y, NAVY) end end
fillSpans(floorClean, { {y=2, spans={{2,3,STONE}}}, {y=3, spans={{2,3,STONE}}} })

local floorCracked = blank(16, 16)
for y = 0, 15 do for x = 0, 15 do floorCracked:drawPixel(x, y, NAVY) end end
fillSpans(floorCracked, {
  {y=2,  spans={{2,3,STONE}}}, {y=3, spans={{2,3,STONE}}},
  {y=6,  spans={{3,4,VOID}}},
  {y=7,  spans={{5,6,VOID}}},
  {y=8,  spans={{6,7,VOID}}},
  {y=9,  spans={{7,8,VOID}}},
  {y=10, spans={{8,8,VOID}}},
})

local wallEdgeLeft = blank(16, 16)
for y = 0, 15 do
  for x = 0, 15 do
    if x < 3 then wallEdgeLeft:drawPixel(x, y, VOID)
    else wallEdgeLeft:drawPixel(x, y, STONE) end
  end
end

local wallEdgeRight = blank(16, 16)
for y = 0, 15 do
  for x = 0, 15 do
    if x >= 13 then wallEdgeRight:drawPixel(x, y, VOID)
    else wallEdgeRight:drawPixel(x, y, STONE) end
  end
end

local wallCap = blank(16, 16)
for y = 0, 15 do
  for x = 0, 15 do
    if y < 3 then wallCap:drawPixel(x, y, VOID)
    else wallCap:drawPixel(x, y, STONE) end
  end
end

local voidStrip = blank(16, 32)
for y = 0, 31 do
  for x = 0, 15 do
    local c = VOID
    if y >= 20 then c = NAVY
    elseif y >= 14 then c = VOID end
    voidStrip:drawPixel(x, y, c)
  end
end

local envSourcePath = srcDir .. "/env_corridor_microkit.aseprite"
local envSpr = Sprite(16, 16, ColorMode.RGB)
envSpr.filename = envSourcePath
local names = {"FloorClean", "FloorCracked", "WallEdgeLeft", "WallEdgeRight", "WallCap"}
local imgs = {floorClean, floorCracked, wallEdgeLeft, wallEdgeRight, wallCap}
local layer1 = envSpr.layers[1]
layer1.name = names[1]
local cel1 = layer1:cel(1)
if cel1 == nil then cel1 = envSpr:newCel(layer1, 1) end
cel1.image = imgs[1]
for i = 2, #names do
  local l = envSpr:newLayer()
  l.name = names[i]
  local c = envSpr:newCel(l, 1)
  c.image = imgs[i]
end
-- VoidStrip is a different canvas height (32) -- kept as its own sprite/source
-- below rather than forced into this 16x16 document.
checkOverwrite(envSourcePath)
envSpr:saveAs(envSourcePath)
print("Saved source: " .. envSourcePath)

local envExports = {
  {layer = "FloorClean", path = envDir .. "/env_floor_clean.png"},
  {layer = "FloorCracked", path = envDir .. "/env_floor_cracked.png"},
  {layer = "WallEdgeLeft", path = envDir .. "/env_wall_edge_left.png"},
  {layer = "WallEdgeRight", path = envDir .. "/env_wall_edge_right.png"},
  {layer = "WallCap", path = envDir .. "/env_wall_cap.png"},
}
for _, exp in ipairs(envExports) do
  for _, l in ipairs(envSpr.layers) do l.isVisible = (l.name == exp.layer) end
  checkOverwrite(exp.path)
  envSpr:saveCopyAs(exp.path)
  print("Exported: " .. exp.path)
end
for _, l in ipairs(envSpr.layers) do l.isVisible = (l.name == names[1]) end
envSpr:close()

-- void strip as its own 16x32 sprite, saved into the same-named source group
-- via a second file (different canvas size from the 16x16 kit above).
buildAndExport(16, 32, "VoidStrip", voidStrip,
  srcDir .. "/env_void_strip.aseprite", envDir .. "/env_void_strip.png")

print("")
print("Batch 1 complete.")
