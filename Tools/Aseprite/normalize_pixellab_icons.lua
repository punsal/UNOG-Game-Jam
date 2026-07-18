--[[
ART-06 -- Cost icon set normalization.

Takes the 5 raw 64x64 PixelLab references (Assets/Art/Generated/References/
ART-06/) as shape reference only. Downscales to 24x24 by nearest-neighbour
point sampling, then flattens every opaque pixel to a single flat approved
color per icon (alpha thresholded 0/255) -- strongest possible silhouette
read at 24x24, matching the project's flat hard-edge style. Shape alone
carries the read; color is a secondary cue, not required to distinguish
icons (satisfies "understandable without color").
--]]

local function scriptDir()
  local src = debug.getinfo(1, "S").source
  local path = src:match("^@(.*)$") or src
  return path:match("^(.*)[/\\][^/\\]+$")
end
local root = scriptDir() .. "/../.."
local refDir = root .. "/Assets/Art/Generated/References/ART-06"
local srcDir = root .. "/Assets/Art/Source/Aseprite"
local prodDir = root .. "/Assets/Art/Production/UI"
os.execute(string.format("mkdir -p '%s'", srcDir))
os.execute(string.format("mkdir -p '%s'", prodDir))

local function checkOverwrite(path)
  if app.fs.isFile(path) then print("NOTICE: overwriting existing file: " .. path) end
end

local RED    = app.pixelColor.rgba(217, 74, 90, 255)   -- Danger Red
local PURPLE = app.pixelColor.rgba(117, 89, 142, 255)  -- Shadow Purple
local BONE   = app.pixelColor.rgba(217, 225, 232, 255) -- Bone
local CLEAR  = app.pixelColor.rgba(0, 0, 0, 0)

local ICONS = {
  {ref = "icon_health_RAW.png",    out = "ui_icon_health.png",    color = RED,    layer = "Health"},
  {ref = "icon_light_RAW.png",     out = "ui_icon_light.png",     color = BONE,   layer = "Light"},
  {ref = "icon_speed_RAW.png",     out = "ui_icon_speed.png",     color = BONE,   layer = "Speed"},
  {ref = "icon_vision_RAW.png",    out = "ui_icon_vision.png",    color = PURPLE, layer = "Vision"},
  {ref = "icon_body_size_RAW.png", out = "ui_icon_body_size.png", color = BONE,   layer = "BodySize"},
}

local SRC_SIZE, DST_SIZE = 64, 24

local function downscaleFlatten(rawImg, color)
  local img = Image(DST_SIZE, DST_SIZE, ColorMode.RGB)
  for y = 0, DST_SIZE - 1 do
    for x = 0, DST_SIZE - 1 do
      local sx = math.floor(x * SRC_SIZE / DST_SIZE + 0.5)
      local sy = math.floor(y * SRC_SIZE / DST_SIZE + 0.5)
      if sx > SRC_SIZE - 1 then sx = SRC_SIZE - 1 end
      if sy > SRC_SIZE - 1 then sy = SRC_SIZE - 1 end
      local c = rawImg:getPixel(sx, sy)
      if app.pixelColor.rgbaA(c) >= 128 then
        img:drawPixel(x, y, color)
      else
        img:drawPixel(x, y, CLEAR)
      end
    end
  end
  return img
end

local sourcePath = srcDir .. "/ui_cost_icons.aseprite"
local spr = Sprite(DST_SIZE, DST_SIZE, ColorMode.RGB)
spr.filename = sourcePath

local firstDone = false
for i, icon in ipairs(ICONS) do
  local refPath = refDir .. "/" .. icon.ref
  if not app.fs.isFile(refPath) then
    error("Reference not found: " .. refPath)
  end
  local rawSpr = app.open(refPath)
  local rawImg = rawSpr.layers[1]:cel(1).image
  local outImg = downscaleFlatten(rawImg, icon.color)
  rawSpr:close()

  local layer
  if not firstDone then
    layer = spr.layers[1]
    firstDone = true
  else
    layer = spr:newLayer()
  end
  layer.name = icon.layer
  local cel = layer:cel(1)
  if cel == nil then cel = spr:newCel(layer, 1) end
  cel.image = outImg
end

for _, l in ipairs(spr.layers) do l.isVisible = (l.name == "Health") end
checkOverwrite(sourcePath)
spr:saveAs(sourcePath)
print("Saved source: " .. sourcePath)

for _, icon in ipairs(ICONS) do
  for _, l in ipairs(spr.layers) do l.isVisible = (l.name == icon.layer) end
  local outPath = prodDir .. "/" .. icon.out
  checkOverwrite(outPath)
  spr:saveCopyAs(outPath)
  print("Exported: " .. outPath)
end

for _, l in ipairs(spr.layers) do l.isVisible = (l.name == "Health") end
spr:close()
print("Done.")
