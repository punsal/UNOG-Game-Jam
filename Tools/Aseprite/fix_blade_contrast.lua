--[[
ART-04 -- Moving blade contrast fix.

Feedback: the blade's dark Stone outer mass blended into the dark corridor
background, so the cross silhouette didn't read immediately. Same geometry
as the original generation (Tools/Aseprite/generate_batch1_deterministic_assets.lua),
recolored with brighter blade-specific Stone/Purple shades (blended ~18%
toward Bone) so the cross reads instantly against Void/Deep Navy backgrounds.
This does NOT change the shared Stone/Shadow Purple palette tokens used
elsewhere (spikes, corridor kit, altars) -- only this asset's two colors.
--]]

local function scriptDir()
  local src = debug.getinfo(1, "S").source
  local path = src:match("^@(.*)$") or src
  return path:match("^(.*)[/\\][^/\\]+$")
end
local root = scriptDir() .. "/../.."
local srcDir = root .. "/Assets/Art/Source/Aseprite"
local hazDir = root .. "/Assets/Art/Production/Hazards"

local function checkOverwrite(path)
  if app.fs.isFile(path) then print("NOTICE: overwriting existing file: " .. path) end
end

-- Blade-specific brightened shades (~18% blended toward Bone #D9E1E8 from the
-- base Stone/Shadow Purple tokens), for contrast against the dark background.
local STONE_BRIGHT  = app.pixelColor.rgba(74, 81, 96, 255)   -- was Stone #2A3142
local PURPLE_BRIGHT = app.pixelColor.rgba(136, 114, 158, 255) -- was Shadow Purple #75598E

local function blank(w, h)
  local img = Image(w, h, ColorMode.RGB)
  for y = 0, h - 1 do
    for x = 0, w - 1 do img:drawPixel(x, y, app.pixelColor.rgba(0,0,0,0)) end
  end
  return img
end

local bladeImg = blank(32, 32)
local cx, cy = 15.5, 15.5
for y = 0, 31 do
  for x = 0, 31 do
    local dx, dy = x - cx, y - cy
    local onH = math.abs(dy) <= 3 and math.abs(dx) <= 13.5
    local onV = math.abs(dx) <= 3 and math.abs(dy) <= 13.5
    if onH or onV then
      local thickness = 3
      if onH then
        local reach = 13.5 - math.abs(dx)
        if reach < 4 then thickness = math.max(0, math.floor(reach * 0.75)) end
        if math.abs(dy) <= thickness then bladeImg:drawPixel(x, y, STONE_BRIGHT) end
      end
      if onV then
        local reach = 13.5 - math.abs(dy)
        local thicknessV = 3
        if reach < 4 then thicknessV = math.max(0, math.floor(reach * 0.75)) end
        if math.abs(dx) <= thicknessV then bladeImg:drawPixel(x, y, STONE_BRIGHT) end
      end
    end
  end
end
for y = 0, 31 do
  for x = 0, 31 do
    local dist = math.abs(x - cx) + math.abs(y - cy)
    if dist <= 3.5 then bladeImg:drawPixel(x, y, PURPLE_BRIGHT) end
  end
end

local sourcePath = srcDir .. "/haz_blade_base.aseprite"
local pngPath = hazDir .. "/haz_blade_base.png"

local spr = Sprite(32, 32, ColorMode.RGB)
spr.filename = sourcePath
local layer = spr.layers[1]
layer.name = "BladeBase"
local cel = layer:cel(1)
if cel == nil then cel = spr:newCel(layer, 1) end
cel.image = bladeImg

checkOverwrite(sourcePath)
spr:saveAs(sourcePath)
print("Saved source: " .. sourcePath)
checkOverwrite(pngPath)
spr:saveCopyAs(pngPath)
print("Exported: " .. pngPath)
spr:close()
print("Done.")
