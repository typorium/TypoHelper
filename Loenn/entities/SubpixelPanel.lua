local utils = require("utils")


local SubpixelPanel = {}

SubpixelPanel.name = "Typorium_TypoHelper_Entity_SubpixelPanel"
SubpixelPanel.depth = 1

SubpixelPanel.minimumSize = {8, 8}

SubpixelPanel.placements = {
    name = "default",
    data = {
        width = 8,
        height = 8,

        color = "ffffff",
        opacity = 1.0
    }
}

SubpixelPanel.fieldInformation = {
    width = {
        minimumValue = 8
    },
    height = {
        minimumValue = 8
    },
    color = {
        fieldType = "color"
    },
    opacity = {
        minimumValue = 0.0,
        maximumValue = 1.0
    }
}

SubpixelPanel.color = function(room, entity)

    -- Gets color
    local color = entity.color or "ffffff"
    local color_in_table = utils.getColor(color)

    -- If color table has alpha attribute
    if not (color_in_table[4]) then

        -- Change it
        color_in_table[4] = 0.4

    end

    -- Returns color
    return color_in_table
end


return SubpixelPanel