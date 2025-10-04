local utils = require("utils")


local EntitySpecificWind = {}

EntitySpecificWind.name = "Typorium_TypoHelper_Entity_EntitySpecificWind"
EntitySpecificWind.depth = -1

EntitySpecificWind.placements = {
    name = "default",
    data = {
        width = 8,
        height = 8,

        color = "ffffff",
        opacity = 1.0,

        wind = "None",

        account_for_level_wind = false,
        can_player_interact = false,
        can_entities_interact = true,

        compatibility_mode = false
    }
}

EntitySpecificWind.fieldInformation = {
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
    },

    wind = {
        editable = false,
        options = {
            {"None", "none"},
            {"Left", "left"},
            {"Right", "right"},
            {"Left (Strong)", "left_strong"},
            {"Right (Strong)", "right_strong"},
            {"Left (On/Off)", "left_onoff"},
            {"Right (On/Off)", "right_onoff"},
            {"Left (Strong) (On/Off)", "left_fast_onoff"},
            {"Right (Strong) (On/Off)", "right_fast_onoff"},
            {"Alternating", "alternating"},
            {"Right (Crazy)", "right_crazy"},
            {"Down", "down"},
            {"Up", "up"},
            {"Space", "space"}
        }
    }
}

EntitySpecificWind.color = function(room, entity)

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

return EntitySpecificWind