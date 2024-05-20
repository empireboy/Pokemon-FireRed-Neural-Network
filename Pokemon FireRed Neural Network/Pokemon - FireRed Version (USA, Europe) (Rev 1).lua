socket = require("socket")

-- LUA GENERAL VARIABLES

DataTypeSize = {
    u8 = 1,
	s8 = 2,
    u16 = 3,
	s16 = 4,
    u32 = 5,
	s32 = 6
}

-- POKEMON PROJECT VARIABLES

DrawMode = {
	None = 1,
	Inputs = 2,
	InputsFullscreen = 3,
	Outputs = 4,
	Statistics = 5,
}

debugMode = true
isDebugPauseActive = false
drawMode = DrawMode.Inputs

Input = {}

input = nil
inputScannerWidth = 9
inputScannerHeight = 9
output = {}
previousOutput = {}

OutputType = {
	Left = 1,
	Right = 2,
	Up = 3,
	Down = 4,
	A = 5,
}

joypadTable = {}
joypadControl = true
isActionKeyPressed = false

playerPositionX = 0
playerPositionY = 0
playerPositionXAddress = 0x036E4C
playerPositionYAddress = 0x036E4E
previousPlayerPositionX = 0
previousPlayerPositionY = 0
playerNextPositionX = 0
playerNextPositionY = 0
playerNextPositionXAddress = 0x036E48
playerNextPositionYAddress = 0x036E4A
playerRotation = 0
playerRotationAddress = 0x036E50
previousPlayerRotation = 0
isPlayerWalkingAgainstWall = 0
isPlayerWalkingAgainstWallAddress = 0x0473A3
previousIsPlayerWalkingAgainstWall = 0
playerLookatTileFromDialogue = nil
playerLookatTileFromDialogueX = nil
playerLookatTileFromDialogueY = nil
playerTileFromDialogue = nil
playerTileFromDialogueX = nil
playerTileFromDialogueY = nil
playerDistanceMoved = 0
playerDistanceMovedSteps = 20
playerDistanceMovedCurrentSteps = 0
playerDistanceMovedStartX = nil
playerDistanceMovedStartY = nil

-- These variables might be gen 3 specific
DialogueType = {
	None = 0,
	Default = 1,
	Confirmation = 2
}

MapEvents = {
    -- Object Events
    objectEventCount = 0,
    objectEventsAddress = nil,
    
    -- Warp Events
    warpCount = 0,
    warpAddress = nil,
    
    -- Coord Events
    coordEventCount = 0,
    coordEventsAddress = nil,
    
    -- Background Events
    backgroundEventCount = 0,
    backgroundEventsAddress = nil,
    
    -- Offsets and Types
    objectEventCountOffset		= 0x00,
	objectEventCountType		= DataTypeSize.u8,
    warpCountOffset				= 0x01,
	warpCountType				= DataTypeSize.u8,
    coordEventCountOffset		= 0x02,
	coordEventCountType			= DataTypeSize.u8,
    backgroundEventCountOffset	= 0x03,
    backgroundEventCountType	= DataTypeSize.u8,
    objectEventsOffset			= 0x04,
	objectEventsType			= DataTypeSize.u32,
    warpsOffset					= 0x08,
	warpsType					= DataTypeSize.u32,
    coordEventsOffset			= 0x0C,
	coordEventsType				= DataTypeSize.u32,
    backgroundEventsOffset		= 0x10,
	backgroundEventsType		= DataTypeSize.u32
}

ObjectEvent = {
	-- Offsets and Types
	localIDOffset			= 0x00,
	localIDType				= DataTypeSize.u8,
	graphicsIDOffset		= 0x01,
	graphicsIDType			= DataTypeSize.u8,
	xOffset					= 0x02,
	xType					= DataTypeSize.s16,
	yOffset					= 0x04,
	yType					= DataTypeSize.s16,

	objectEventOffset		= 0x34
}

WarpEvent = {
	-- Offsets and Types
	xOffset					= 0x00,
	xType					= DataTypeSize.s16,
	yOffset					= 0x02,
	yType					= DataTypeSize.s16,
	elevationOffset			= 0x04,
	elevationType			= DataTypeSize.u8,

	warpOffset				= 0x08
}

CoordEvent = {
	-- Offsets and Types
	xOffset					= 0x00,
	xType					= DataTypeSize.u16,
	yOffset					= 0x02,
	yType					= DataTypeSize.u16,
	elevationOffset			= 0x04,
	elevationType			= DataTypeSize.u8,

	coordEventOffset		= 0x08
}

BackgroundEvent = {
	-- Offsets and Types
	xOffset					= 0x00,
	xType					= DataTypeSize.u16,
	yOffset					= 0x02,
	yType					= DataTypeSize.u16,
	elevationOffset			= 0x04,
	elevationType			= DataTypeSize.u8,

	backgroundEventOffset	= 0x0C
}

Character = {
	previousPositionX = {},
	previousPositionY = {},

	-- Offsets and Types
	baseAddress				= 0x02036E38,

	xNextOffset				= 0x10,
	xNextType				= DataTypeSize.u16,
	yNextOffset				= 0x12,
	yNextType				= DataTypeSize.u16,
	xOffset					= 0x14,
	xType					= DataTypeSize.u16,
	yOffset					= 0x16,
	yType					= DataTypeSize.u16,

	characterOffset			= 0x24
}

playerInDialogueType = DialogueType.None
playerInDialogueTypeAddress = 0x040EA8
previousPlayerInDialogueType = DialogueType.None
isPlayerAbleToGoNextDialogue = false
isPlayerAbleToGoNextDialogueAddress1 = 0x020050
isPlayerAbleToGoNextDialogueAddress2 = 0x02004F
isPlayerAbleToGoNextDialogue3 = 0
isPlayerAbleToGoNextDialogue3Lock = false
isPlayerAbleToGoNextDialogueAddress3 = 0x040EB0
previousPlayerAbleToGoNextDialogue3 = 0

currentMapEventsPointer = 0x02036E00
currentMapEventsAddress = nil

mapEventsPositionOffset = 7
-- END

Colors = {
    ["Black"] = "#000000",
    ["Silver"] = "#C0C0C0",
    ["Gray"] = "#808080",
    ["White"] = "#FFFFFF",
    ["Maroon"] = "#800000",
    ["Red"] = "#FF0000",
    ["Purple"] = "#800080",
    ["Fuchsia"] = "#FF00FF",
    ["Green"] = "#008000",
    ["Lime"] = "#00FF00",
    ["Olive"] = "#808000",
    ["Yellow"] = "#FFFF00",
    ["Navy"] = "#000080",
    ["Blue"] = "#0000FF",
    ["Teal"] = "#008080",
    ["Aqua"] = "#00FFFF",
	["Pink"] = "#FFC0CB",
	["Lemon"] = "#FF8C00",
}

RotationType = {
	Left = 1,
	Right = 2,
	Up = 3,
	Down = 4
}

TileType = {
	Unexplored = 0,
	Explored = 1,
	Wall = 2,
	ObjectEvent = 3,
	Warp = 4,
	CoordEvent = 5,
	BackgroundEvent = 6,
	NPC = 7,
	DialogueFinished = 8,
	--Grass = 4
}

TileTypeColor = {
	Unexplored = "Red",
	Explored = "Green",
	Wall = "White",
	ObjectEvent = "Olive",
	Warp = "Blue",
	CoordEvent = "Aqua",
	BackgroundEvent = "Yellow",
	NPC = "Lemon",
	DialogueFinished = "Pink",
	Grass = "DarkGreen",
}

Analytics = {
	["BestCorrectSteps"] = 0,
	["CurrentCorrectSteps"] = 0,
}

currentReward = 0

runTime = 30000
runTimer = 0
isRunTimerActive = true

stuckTime = 500 --80
stuckTimer = 0
isStuckTimerActive = true

explorationExploitationLinear = 0
explorationExploitationTanh = 0
explorationExploitationTanhMax = 90
explorationExploitationIncrease = 0.007
explorationExploitationLock = false

saveState = 9
minSaveState = 5
maxSaveState = 8
isRandomSaveStateActive = false

extraBorderWidth = 400
isExtraBorderActive = false

trainingServerAddress = "127.0.0.1"
trainingServerPort = 65432
trainingClient = nil
isTrainingClientConnected = false
offlineMode = false
maxReconnectAttempts = 10

-- POKEMON GEN 3 SPECIFIC VARIABLES

worldDictionary = {}
mapTileWidth = 80
mapTileHeight = 80
mapTilePosition = 0
mapTilePositionAddress = 0x036DFC

-- LUA GENERAL METHODS

function randomSeed()
	math.randomseed(os.clock()*1000000)
end

function copyTable(fromTable, toTable)
	for i, value in pairs(fromTable) do
		toTable[i] = value
	end
end

function boolToNumber(value)
	return value and 1 or 0
end

function numberToBool(value)
	if value == 1 then
		return true
	elseif value == 0 then
		return false
	end

	return false
end

function manhattanDistance(x1, y1, x2, y2)
	return math.abs(x1 - x2) + math.abs(y1 - y2)
end

function createColorWithAlpha(colorName, alpha)
    local hexColor = Colors[colorName]

    if hexColor then
        local colorPart = hexColor:sub(2)

        return string.format("#%02X%s", alpha, colorPart)
    else
        print("Color not found: ", colorName)

        return nil
    end
end

function setJoypadInput(joypadTable)
	joypad.set(joypadTable)
end

function isInGridRange(x, y, width, height)
	if 
		x >= 1 and
		x <= width and
		y >= 1 and
		y <= height
	then
		return true
	end

	return false
end

function isTableEqual(table1, table2)
	local tableEqual = true

	for i = 1, #(table1) do
		if table1[i] ~= table2[i] then
			tableEqual = false
		end
	end

	return tableEqual
end

function getEnumSize(enum)
    local size = 0

    for _ in pairs(enum) do
        size = size + 1
    end

    return size
end

function getMaxAndIndexFromTable(fromTable)
	local maxValue = -math.huge
	local maxIndex = nil

	for index, value in ipairs(fromTable) do
		if value > maxValue then
			maxValue = value
			maxIndex = index
		end
	end

	return maxValue, maxIndex
end

function getMaxIndexFromTable(fromTable)
	local maxValue, maxIndex = getMaxAndIndexFromTable(fromTable)

	return maxIndex
end

function readFromMemory(address, offset, dataTypeSize)
	if dataTypeSize == DataTypeSize.u8 then
		return memory.read_u8(address + offset)

	elseif dataTypeSize == DataTypeSize.s8 then
		return memory.read_s8(address + offset)

	elseif dataTypeSize == DataTypeSize.u16 then
		return memory.read_u16_le(address + offset)

	elseif dataTypeSize == DataTypeSize.s16 then
		return memory.read_s16_le(address + offset)

	elseif dataTypeSize == DataTypeSize.u32 then
		return memory.read_u32_le(address + offset)

	elseif dataTypeSize == DataTypeSize.s32 then
		return memory.read_s32_le(address + offset)

	end

	return -1
end

-- POKEMON PROJECT METHODS

function printInputScanner(input)
	local inputScannerStrings = {}
	local inputScannerIndex = 1

	for i = 1, inputScannerWidth do
		for j = 1, inputScannerHeight do
			if inputScannerStrings[j] == nil then
				inputScannerStrings[j] = ""
			end

			inputScannerStrings[j] = inputScannerStrings[j] .. tostring(input[inputScannerIndex])

			inputScannerIndex = inputScannerIndex + 1
		end
	end

	for i = 1, #(inputScannerStrings) do
		print(inputScannerStrings[i])
	end

	print(inputScannerBorderString)
end

function takeControl(userControl)
	if userControl then
		joypadControl = true
		isStuckTimerActive = false
		runTime = 999999
	else
		joypadControl = false
		isStuckTimerActive = true
		runTime = 30000
	end
end

function updateFromMemory()
	memory.usememorydomain("Combined WRAM")
	previousPlayerRotation = playerRotation
	previousIsPlayerWalkingAgainstWall = isPlayerWalkingAgainstWall
	previousPlayerInDialogueType = playerInDialogueType

	playerPositionX = memory.read_u8(playerPositionXAddress)
	playerPositionY = memory.read_u8(playerPositionYAddress)
	playerNextPositionX = memory.read_u8(playerNextPositionXAddress)
	playerNextPositionY = memory.read_u8(playerNextPositionYAddress)
	playerRotation = playerRotationToRotationType(memory.read_u8(playerRotationAddress))
	isPlayerWalkingAgainstWall = playerWalkingAgainstWallToBool(memory.read_u8(isPlayerWalkingAgainstWallAddress))

	playerInDialogueType = playerInDialogueValueToType(memory.read_u8(playerInDialogueTypeAddress))
	local isPlayerAbleToGoNextDialogue1 = memory.read_u8(isPlayerAbleToGoNextDialogueAddress1)
	local isPlayerAbleToGoNextDialogue2 = memory.read_u8(isPlayerAbleToGoNextDialogueAddress2)

	if isPlayerAbleToGoNextDialogue3Lock == false then
		previousPlayerAbleToGoNextDialogue3 = isPlayerAbleToGoNextDialogue3
	end

	isPlayerAbleToGoNextDialogue3 = memory.read_u16_le(isPlayerAbleToGoNextDialogueAddress3)

	if isPlayerAbleToGoNextDialogue2 ~= 0 then
		isPlayerAbleToGoNextDialogue3Lock = false
	end

	if isPlayerAbleToGoNextDialogue2 == 0
		and isPlayerAbleToGoNextDialogue3 > previousPlayerAbleToGoNextDialogue3
	then
		isPlayerAbleToGoNextDialogue3Lock = true
	end

	isPlayerAbleToGoNextDialogue = playerAbleToGoNextDialogueToBool(
		isPlayerAbleToGoNextDialogue1,
		isPlayerAbleToGoNextDialogue2,
		isPlayerAbleToGoNextDialogue3Lock
	)
	mapTilePosition = memory.read_s32_le(mapTilePositionAddress)

	memory.usememorydomain("System Bus")
	local mapEventsAddress = memory.read_u32_le(currentMapEventsPointer)

	-- Only update MapEvents once you enter a new Map
	if mapEventsAddress ~= currentMapEventsAddress then
		updateCurrentMapEvents(mapEventsAddress, MapEvents)
		removeNPCTilesFromCurrentMapTile()
	end

	currentMapEventsAddress = mapEventsAddress

	memory.usememorydomain("Combined WRAM")
end

function playerWalkingAgainstWallToBool(value)
	if value == 0 then
		return true
	elseif value == 1 then
		return false
	end

	return false
end

function playerRotationToRotationType(value)
	if value == 51 then
		return RotationType.Left
	elseif value == 68 then
		return RotationType.Right
	elseif value == 34 then
		return RotationType.Up
	elseif value == 17 then
		return RotationType.Down
	end

	return RotationType.Left
end

function playerInDialogueValueToType(value)
	if value == 0 then
		return DialogueType.Default
	elseif value == 1 then
		return DialogueType.Confirmation
	elseif value == 2 then
		return DialogueType.None
	end

	return DialogueType.None
end

function playerAbleToGoNextDialogueToBool(value1, value2, value3)
	if playerInDialogueType == DialogueType.None then
		return false
	elseif value1 == 2 then
		return true
	elseif value2 == 0
		and value3 == true
	then
		return true
	end

	return false
end

function gameCenterX()
	local extraWidth = extraBorderWidth

	if not isExtraBorderActive then
		extraWidth = 0
	end

	return (client.screenwidth() + extraWidth) / 2
end

function gameCenterY()
	return client.screenheight() / 2
end

function createDefaultMapTile()
	return createMapTile(mapTileWidth, mapTileHeight, 0)
end

function createMapTile(width, height, initialTileValue)
	local mapTile = {}

	for i = 1, mapTileWidth do
		mapTile[i] = {}

		for j = 1, mapTileHeight do
			mapTile[i][j] = 0
		end
	end

	return mapTile
end

function isInMapTileRange(x, y)
	return isInGridRange(x, y, mapTileWidth, mapTileHeight)
end

function isPlayerInMapTileRange()
	return isInMapTileRange(playerPositionX, playerPositionY)
end

function setCurrentPlayerTile(tileType)
	getCurrentMapTile()[playerPositionX][playerPositionY] = tileType
end

function setCurrentPlayerLookatTile(tileType)
	local offsetX, offsetY = getOffsetFromPlayerRotation()

	setTileFromCurrentMapTile(playerPositionX + offsetX, playerPositionY + offsetY, tileType)
end

function setTileFromCurrentMapTile(x, y, tileType)
	getCurrentMapTile()[x][y] = tileType
end

function getOffsetFromPlayerRotation()
	local offsetX
	local offsetY

	if playerRotation == RotationType.Left then
		offsetX = -1
		offsetY = 0
	elseif playerRotation == RotationType.Right then
		offsetX = 1
		offsetY = 0
	elseif playerRotation == RotationType.Up then
		offsetX = 0
		offsetY = -1
	elseif playerRotation == RotationType.Down then
		offsetX = 0
		offsetY = 1
	end

	return offsetX, offsetY
end

function getPlayerTile()
	if not isInMapTileRange(playerPositionX, playerPositionY) then
		return nil
	end

	return getCurrentMapTile()[playerPositionX][playerPositionY]
end

function getPlayerTilePosition()
	return playerPositionX, playerPositionY
end

function getPlayerLookatTile()
	local offsetX, offsetY = getOffsetFromPlayerRotation()

	return getTileFromCurrentMapTile(playerPositionX + offsetX, playerPositionY + offsetY)
end

function getPlayerLookatTilePosition()
	local offsetX, offsetY = getOffsetFromPlayerRotation()

	return playerPositionX + offsetX, playerPositionY + offsetY
end

function getPlayerWalktoDirection()
	local offsetX = playerNextPositionX - playerPositionX
	local offsetY = playerNextPositionY - playerPositionY

	local result = nil

	if offsetX == -1 and offsetY == 0 then
		result = RotationType.Left

	elseif offsetX == 1 and offsetY == 0 then
		result = RotationType.Right

	elseif offsetX == 0 and offsetY == -1 then
		result = RotationType.Up

	elseif offsetX == 0 and offsetY == 1 then
		result = RotationType.Down
	end

	return result
end

function getCurrentMapTile()
	return worldDictionary[mapTilePosition]
end

function getTileFromCurrentMapTile(x, y)
	if not isInMapTileRange(x, y) then
		return nil
	end

	return getCurrentMapTile()[x][y]
end

function getNeuralNetworkInputFromPlayerMapTile()
	local input = {}
	local inputIndex = 1

	for i = 1, inputScannerWidth do
		for j = 1, inputScannerHeight do
			local tileX = playerPositionX - math.floor(inputScannerWidth / 2) + i - 1
			local tileY = playerPositionY - math.floor(inputScannerHeight / 2) + j - 1

			if isInMapTileRange(tileX, tileY) then
				input[inputIndex] = getCurrentMapTile()[tileX][tileY]
			else
				input[inputIndex] = TileType.Wall
			end

			input[inputIndex] = input[inputIndex]

			inputIndex = inputIndex + 1
		end
	end

	return input
end

function getNeuralNetworkInputFromPlayerMapTileSection(x, y, width, height)
	local input = {}
	local inputIndex = 1

	for i = 1, width do
		for j = 1, height do
			local tileX = x - math.floor(width / 2) + i - 1
			local tileY = y - math.floor(height / 2) + j - 1

			if isInMapTileRange(tileX, tileY) then
				input[inputIndex] = getCurrentMapTile()[tileX][tileY]
			else
				input[inputIndex] = TileType.Wall
			end

			input[inputIndex] = input[inputIndex]

			inputIndex = inputIndex + 1
		end
	end

	return input
end

function getNeuralNetworkInputFromPlayerMapTileNextPosition()
	local input = {}
	local inputIndex = 1

	for i = 1, inputScannerWidth do
		for j = 1, inputScannerHeight do
			local tileX = playerNextPositionX - math.floor(inputScannerWidth / 2) + i - 1
			local tileY = playerNextPositionY - math.floor(inputScannerHeight / 2) + j - 1

			if isInMapTileRange(tileX, tileY) then
				input[inputIndex] = getCurrentMapTile()[tileX][tileY]
			else
				input[inputIndex] = TileType.Wall
			end

			input[inputIndex] = input[inputIndex]

			inputIndex = inputIndex + 1
		end
	end

	return input
end

function getNeuralNetworkInputFromPlayerInDialogueType()
	return playerInDialogueType
end

function getNeuralNetworkInputFromPlayerAbleToGoNextDialogue()
	return isPlayerAbleToGoNextDialogue
end

function getNeuralNetworkInputFromPlayerRotation()
	return playerRotation
end

function getNeuralNetworkInputFromPlayerDistanceMoved()
	return playerDistanceMoved
end

function getOutputSize()
	return getEnumSize(OutputType)
end

function removeExploredTilesFromWorld()
	-- Loop through all Map Tiles
	for _, value in pairs(worldDictionary) do

		-- Loop through all tiles on current Map Tile
		for x = 1, #value do
			for y = 1, #value[x] do

				-- Only remove Explored tiles. DialogueFinished tiles are also explored
				if value[x][y] == TileType.Explored
					or value[x][y] == TileType.DialogueFinished
				then
					value[x][y] = TileType.Unexplored
				end
			end
		end
	end
end

function removeNPCTilesFromCurrentMapTile()
	local currentMapTile = getCurrentMapTile()

	if not currentMapTile then
		return
	end

	-- Loop through all tiles on current Map Tile
	for x = 1, #currentMapTile do
		for y = 1, #currentMapTile[x] do

			-- Only remove NPC tiles
			if (currentMapTile[x][y] == TileType.NPC) then
				currentMapTile[x][y] = TileType.Unexplored
			end
		end
	end
end

function updateCurrentMapEvents(currentMapEventsAddress, mapEvents)
	mapEvents.objectEventCount = readFromMemory(
		currentMapEventsAddress,
		mapEvents.objectEventCountOffset,
		mapEvents.objectEventCountType
	)
	mapEvents.objectEventsAddress = readFromMemory(
		currentMapEventsAddress,
		mapEvents.objectEventsOffset,
		mapEvents.objectEventsType
	)
	mapEvents.warpCount = readFromMemory(
		currentMapEventsAddress,
		mapEvents.warpCountOffset,
		mapEvents.warpCountType
	)
	mapEvents.warpAddress = readFromMemory(
		currentMapEventsAddress,
		mapEvents.warpsOffset,
		mapEvents.warpsType
	)
	mapEvents.coordEventCount = readFromMemory(
		currentMapEventsAddress,
		mapEvents.coordEventCountOffset,
		mapEvents.coordEventCountType
	)
	mapEvents.coordEventsAddress = readFromMemory(
		currentMapEventsAddress,
		mapEvents.coordEventsOffset,
		mapEvents.coordEventsType
	)
	mapEvents.backgroundEventCount = readFromMemory(
		currentMapEventsAddress,
		mapEvents.backgroundEventCountOffset,
		mapEvents.backgroundEventCountType
	)
	mapEvents.backgroundEventsAddress = readFromMemory(
		currentMapEventsAddress,
		mapEvents.backgroundEventsOffset,
		mapEvents.backgroundEventsType
	)
end

function updateTilesOnCurrentMapTile()
	updateCurrentMapTile()

	updateTilesAroundPlayer()

	--updateObjectEventTiles()

	updateWarpEventTiles()

	updateCoordEventTiles()

	updateBackgroundEventTiles()

	updateNPCTiles()
end

function updateCurrentMapTile()
	if worldDictionary[mapTilePosition] == nil then
		worldDictionary[mapTilePosition] = createDefaultMapTile()
	end
end

function updateTilesAroundPlayer()
	local tileOffsetX = 0
	local tileOffsetY = 0
	local tilePositionX = 0
	local tilePositionY = 0

	--for i = 0, 3 do
		-- Get grid offset by player rotation
		if playerRotation == RotationType.Left then
			tileOffsetX = -1
			tileOffsetY = 0
		elseif playerRotation == RotationType.Right then
			tileOffsetX = 1
			tileOffsetY = 0
		elseif playerRotation == RotationType.Up then
			tileOffsetX = 0
			tileOffsetY = -1
		elseif playerRotation == RotationType.Down then
			tileOffsetX = 0
			tileOffsetY = 1
		end

		tilePositionX = playerPositionX + tileOffsetX
		tilePositionY = playerPositionY + tileOffsetY

		local tile = getTileFromCurrentMapTile(tilePositionX, tilePositionY)

		if isInMapTileRange(tilePositionX, tilePositionY) then
			if isPlayerWalkingAgainstWall
				and previousIsPlayerWalkingAgainstWall ~= isPlayerWalkingAgainstWall
				and previousPlayerRotation == playerRotation
				and tile ~= TileType.ObjectEvent
				and tile ~= TileType.BackgroundEvent
				and tile ~= TileType.Warp
				and tile ~= TileType.CoordEvent
				and tile ~= TileType.NPC
				and tile ~= TileType.DialogueFinished
			then
				setTileFromCurrentMapTile(tilePositionX, tilePositionY, TileType.Wall)
			end
			--[[
			elseif playerInDialogueType == DialogueType.Default
				and previousPlayerInDialogueType == DialogueType.None
			then
				setTileFromCurrentMapTile(tilePositionX, tilePositionY, TileType.BackgroundEvent)
			end
			]]
		end
	--end
end

function updateObjectEventTiles()
	memory.usememorydomain("System Bus")

	for i = 1, MapEvents.objectEventCount do
		local x = readFromMemory(
			MapEvents.objectEventsAddress + ObjectEvent.objectEventOffset * (i - 1),
			ObjectEvent.xOffset,
			ObjectEvent.xType
		)
		local y = readFromMemory(
			MapEvents.objectEventsAddress + ObjectEvent.objectEventOffset * (i - 1),
			ObjectEvent.yOffset,
			ObjectEvent.yType
		)

		x = x + mapEventsPositionOffset
		y = y + mapEventsPositionOffset

		if isInMapTileRange(x, y) then
			setTileFromCurrentMapTile(x, y, TileType.ObjectEvent)
		end
	end
end

function updateWarpEventTiles()
	memory.usememorydomain("System Bus")

	for i = 1, MapEvents.warpCount do
		local x = readFromMemory(
			MapEvents.warpAddress + WarpEvent.warpOffset * (i - 1),
			WarpEvent.xOffset,
			WarpEvent.xType
		)
		local y = readFromMemory(
			MapEvents.warpAddress + WarpEvent.warpOffset * (i - 1),
			WarpEvent.yOffset,
			WarpEvent.yType
		)

		x = x + mapEventsPositionOffset
		y = y + mapEventsPositionOffset

		if isInMapTileRange(x, y) then
			setTileFromCurrentMapTile(x, y, TileType.Warp)
		end
	end
end

function updateCoordEventTiles()
	memory.usememorydomain("System Bus")

	for i = 1, MapEvents.coordEventCount do
		local x = readFromMemory(
			MapEvents.coordEventsAddress + CoordEvent.coordEventOffset * (i - 1),
			CoordEvent.xOffset,
			CoordEvent.xType
		)
		local y = readFromMemory(
			MapEvents.coordEventsAddress + CoordEvent.coordEventOffset * (i - 1),
			CoordEvent.yOffset,
			CoordEvent.yType
		)

		x = x + mapEventsPositionOffset
		y = y + mapEventsPositionOffset

		if isInMapTileRange(x, y)
			and getTileFromCurrentMapTile(x, y) ~= TileType.DialogueFinished
		then
			setTileFromCurrentMapTile(x, y, TileType.CoordEvent)
		end
	end
end

function updateBackgroundEventTiles()
	memory.usememorydomain("System Bus")

	for i = 1, MapEvents.backgroundEventCount do
		local x = readFromMemory(
			MapEvents.backgroundEventsAddress + BackgroundEvent.backgroundEventOffset * (i - 1),
			BackgroundEvent.xOffset,
			BackgroundEvent.xType
		)
		local y = readFromMemory(
			MapEvents.backgroundEventsAddress + BackgroundEvent.backgroundEventOffset * (i - 1),
			BackgroundEvent.yOffset,
			BackgroundEvent.yType
		)

		x = x + mapEventsPositionOffset
		y = y + mapEventsPositionOffset

		if isInMapTileRange(x, y)
			and getTileFromCurrentMapTile(x, y) ~= TileType.DialogueFinished
		then
			setTileFromCurrentMapTile(x, y, TileType.BackgroundEvent)
		end
	end
end

function updateNPCTiles()
	memory.usememorydomain("System Bus")

	-- Start at 2 to exclude the player character
	-- There are 12 spots in memory reserved for characters
	for i = 2, 12 do
		if readFromMemory(
			Character.baseAddress + Character.characterOffset * (i - 1),
			0x00,
			DataTypeSize.u8
		) == 0
		then
			break
		end

		local x = readFromMemory(
			Character.baseAddress + Character.characterOffset * (i - 1),
			Character.xOffset,
			Character.xType
		)
		local y = readFromMemory(
			Character.baseAddress + Character.characterOffset * (i - 1),
			Character.yOffset,
			Character.yType
		)
		local xNext = readFromMemory(
			Character.baseAddress + Character.characterOffset * (i - 1),
			Character.xNextOffset,
			Character.xNextType
		)
		local yNext = readFromMemory(
			Character.baseAddress + Character.characterOffset * (i - 1),
			Character.yNextOffset,
			Character.yNextType
		)

		-- Reset the Tile for the NPCs previous position if it moved
		if Character.previousPositionX[i] and Character.previousPositionY[i] then
			if x == xNext
				and y == yNext
				and (
					Character.previousPositionX[i] ~= x
					or Character.previousPositionY[i] ~= y
				)
			then
				if isInMapTileRange(Character.previousPositionX[i], Character.previousPositionY[i]) then
					setTileFromCurrentMapTile(Character.previousPositionX[i], Character.previousPositionY[i], TileType.Unexplored)
				end
			end
		end

		Character.previousPositionX[i] = x
		Character.previousPositionY[i] = y

		if isInMapTileRange(x, y)
			and getTileFromCurrentMapTile(x, y) ~= TileType.DialogueFinished
		then
			setTileFromCurrentMapTile(x, y, TileType.NPC)
		end
	end
end

function updateInput()
	input:update()
end

function updatePreviousOutput()
	copyTable(output, previousOutput)
end

function updateRewardForDialogue()
	local playerLookatTile = getPlayerLookatTile()
	local playerTile = getPlayerTile()

	-- Going into Dialogue
	if input.inputFromPlayerInDialogueType == DialogueType.Default
		and input.previousInputFromPlayerInDialogueType == DialogueType.None
		and (
			playerLookatTile == TileType.BackgroundEvent
			or playerLookatTile == TileType.NPC
			or playerLookatTile == TileType.CoordEvent
			or playerTile == TileType.CoordEvent
		)
	then
		print("Going into dialogue")
		currentReward = currentReward + 10
		playerLookatTileFromDialogue = playerLookatTile
		playerLookatTileFromDialogueX, playerLookatTileFromDialogueY = getPlayerLookatTilePosition()
		playerTileFromDialogue = playerTile
		playerTileFromDialogueX, playerTileFromDialogueY = getPlayerTilePosition()
	end

	-- Going into Dialogue that was already finished
	if input.inputFromPlayerInDialogueType == DialogueType.Default
		and input.previousInputFromPlayerInDialogueType == DialogueType.None
		and (
			playerLookatTileFromDialogue == TileType.DialogueFinished
			or playerLookatTileFromDialogue == TileType.CoordEvent
			or playerTileFromDialogue == TileType.DialogueFinished
		)
	then
		print("Going into an already finished dialogue")
		currentReward = currentReward - 10
	end

	-- Getting out of Dialogue
	if input.inputFromPlayerInDialogueType == DialogueType.None
		and input.previousInputFromPlayerInDialogueType == DialogueType.Default
		and (
			playerLookatTileFromDialogue == TileType.BackgroundEvent
			or playerLookatTileFromDialogue == TileType.NPC
			or playerLookatTileFromDialogue == TileType.CoordEvent
			or playerTileFromDialogue == TileType.CoordEvent
		)
	then
		print("Going out of dialogue")
		currentReward = currentReward + 20

		if playerLookatTileFromDialogue == TileType.BackgroundEvent
			or playerLookatTileFromDialogue == TileType.NPC
			or playerLookatTileFromDialogue == TileType.CoordEvent
		then
			setTileFromCurrentMapTile(
				playerLookatTileFromDialogueX,
				playerLookatTileFromDialogueY,
				TileType.DialogueFinished
			)
		end

		if playerTileFromDialogue == TileType.CoordEvent then
			setTileFromCurrentMapTile(
				playerTileFromDialogueX,
				playerTileFromDialogueY,
				TileType.DialogueFinished
			)
		end

		playerLookatTileFromDialogue = nil
		playerLookatTileFromDialogueX = nil
		playerLookatTileFromDialogueY = nil
		playerTileFromDialogue = nil
		playerTileFromDialogueX = nil
		playerTileFromDialogueY = nil
	end

	-- Going next Dialogue
	if input.inputFromPlayerAbleToGoNextDialogue == false
		and input.previousInputFromPlayerAbleToGoNextDialogue == true
		and (
			playerLookatTileFromDialogue == TileType.BackgroundEvent
			or playerLookatTileFromDialogue == TileType.NPC
			or playerLookatTileFromDialogue == TileType.CoordEvent
			or playerTileFromDialogue == TileType.CoordEvent
		)
	then
		print("Going next dialogue")
		currentReward = currentReward + 2
	end
end

function updateRewardForDistance()
	local playerManhattanDistanceMoved = manhattanDistance(
		playerDistanceMovedStartX,
		playerDistanceMovedStartY,
		playerPositionX,
		playerPositionY
	)

	playerDistanceMoved = math.min(1, playerManhattanDistanceMoved / playerDistanceMovedSteps)

	if playerDistanceMovedCurrentSteps < playerDistanceMovedSteps then
		return
	end

	-- Update reward
	local distanceReward = -5 + 10 * playerDistanceMoved

	currentReward = currentReward + distanceReward

	print("Gained a reward of " .. distanceReward .. " for distance traveled")

	playerDistanceMovedCurrentSteps = 0
	playerDistanceMoved = 0
	playerDistanceMovedStartX = playerPositionX
	playerDistanceMovedStartY = playerPositionY
end

function updatePlayerPreviousPosition()
	previousPlayerPositionX = playerPositionX
	previousPlayerPositionY = playerPositionY
end

function updateOutputResetOnPlayerRotationBug()
	if not output[OutputType.Left] then
		return
	end

	-- Reset output if rotation does not match with the action taken
	local playerWalktoDirection = getPlayerWalktoDirection()

	if playerWalktoDirection == nil then
		return
	end

	local maxAction, maxIndex = getMaxAndIndexFromTable(output)
	local previousMaxAction, previousMaxIndex = getMaxAndIndexFromTable(previousOutput)

	if maxAction and playerRotation then
		if maxIndex ~= playerWalktoDirection 
			and playerInDialogueType == DialogueType.None
		then
			copyTable(previousOutput, output)
		end
	end
end

function loadRandomSaveStateIfActive()
	local nextSaveState = nil

	if isRandomSaveStateActive then
		nextSaveState = math.random(minSaveState, maxSaveState)
	else
		nextSaveState = saveState
	end

	savestate.loadslot(nextSaveState)
end

function loadRandomSaveState()
	local nextSaveState = math.random(minSaveState, maxSaveState)

	savestate.loadslot(nextSaveState)
end

function loadDefaultSaveState()
	savestate.loadslot(saveState)
end

function explorePlayerTile()
	--fitnessPlayerSpeed = fitnessPlayerSpeed + (stuckTime - stuckTimer) * 0.01

	-- If player moved to a new grid tile, add fitness
	if isPlayerInMapTileRange() then
		if getPlayerTile() == TileType.Unexplored or getPlayerTile() == TileType.Wall then
			setCurrentPlayerTile(TileType.Explored)

			currentReward = currentReward + 1

			Analytics["CurrentCorrectSteps"] = Analytics["CurrentCorrectSteps"] + 1

		elseif getPlayerTile() == TileType.Explored then

			currentReward = currentReward - 0.5
			--[[
			fitnessPlayerMoving = fitnessPlayerMoving + 1

			if debugMode and neuralNetworkOutputType == "Policy" then
				addCorrectAction(true)
			end
		elseif worldGrid[playerPositionX][playerPositionY] == exploredTileInput then
			fitnessPlayerMoving = fitnessPlayerMoving - 0.8

			if debugMode and neuralNetworkOutputType == "Policy" then
				addCorrectAction(false)
			end
		]]
		end
	end
end

function playerMovedOneStep()
	return playerPositionX ~= previousPlayerPositionX or playerPositionY ~= previousPlayerPositionY
end

function saveReplay(input, previousOutput, reward)
	if runTimer <= 0 then
		return
	end

	if #input.value <= 0 then
		return "Cannot save replay since input is nil or empty"
	end

	if #input.previousValue <= 0 then
		return "Cannot save replay since previous input is nil or empty"
	end

	if #previousOutput <= 0 then
		return "Cannot save replay since previous output is nil or empty"
	end

	--[[
	if debugMode and isTableEqual(input.value, input.previousValue) then
		print("Input is equal to previous input while saving replay")
		print("Reward: " .. reward)
		debugPause()
	end
	]]

	--[[
	if debugMode and reward ~= 0 then
		print("Reward: " .. reward)
		debugPause()
	end
	]]

	--[[
	print("Saving Replay")
	print("Reward: " .. reward)
	print("Previous Output:")
	print(previousOutput)
	client.pause()
	]]

	--[[
	if debugMode
		and getMaxIndexFromTable(previousOutput) ~= OutputType.A
		and reward ~= -10
		and getPlayerLookatTile() == TileType.Wall
	then
		print("Replay for walking against a wall does not have the correct reward attached to it. The attached reward is " .. reward)
	end

	if debugMode
		and getMaxIndexFromTable(previousOutput) == OutputType.A
		and reward > 0
	then
		print("Replay for pressing A does not have the correct reward attached to it. The attached reward is " .. reward)
		--debugPause()
	end
	]]

	local message = getMessageToSend("Add Replay", input.previousValue, previousOutput, reward, input.value)

	sendToTrainingServer(message)
end

function saveReplayWalkingAgainstWall()
	local maxAction = getMaxIndexFromTable(output)

	local playerLookatTile = getPlayerLookatTile()

	if playerLookatTile ~= TileType.Wall then
		return
	end
		
	if maxAction == OutputType.A
		and playerInDialogueType == DialogueType.Default
	then
		return
	end

	currentReward = currentReward - 10

	local errorMessage = saveReplay(
		input,
		previousOutput,
		currentReward
	)

	if errorMessage then
		print(errorMessage)
	end
end

function updateSavingReplay()
	if #input.value <= 0 or #input.previousValue <= 0 then
		return
	end
	
	if isTableEqual(input.value, input.previousValue) then
		return
	end

	--[[
	if currentReward == 0 then
		return
	end
	]]

	local errorMessage = saveReplay(
		input,
		previousOutput,
		currentReward
	)

	if errorMessage then
		print(errorMessage)
	end
end

function increaseExplorationExploitationValue()
	if explorationExploitationTanh >= explorationExploitationTanhMax then
		-- Make sure it does not go over the max value
		if explorationExploitationTanh ~= explorationExploitationTanhMax then
			explorationExploitationTanh = explorationExploitationTanhMax
		end

		return
	end

	explorationExploitationLinear = explorationExploitationLinear + explorationExploitationIncrease

	explorationExploitationTanh = math.tanh(explorationExploitationLinear / 100) * 100
end

function updateExplorationExploitation()
	if joypadControl then
		return
	end

	if isTableEqual(input.value, input.previousValue) then
		increaseExplorationExploitationValue()
		return
	end

	if explorationExploitationLock then
		explorationExploitationTanh = 100
	end

	if math.random(0, 100) > explorationExploitationTanh then
		-- Randomize action
		output = getRandomOutput(getOutputSize())
	else
		-- Action based on Neural Network output
		local message = getMessageToSend("Process Inputs", input.value)

		sendToTrainingServer(message)

		output = receiveOutputFromTrainingServer()
	end

	--[[
	if (runTimer <= 0) then
		print("Output for first frame of run:")
		print(output)
	end
	]]

	joypadTable = getJoypadTableFromOutput(output)

	increaseExplorationExploitationValue()
end

function getRandomOutput(buttonCount)
	local joypadTable = {}

	local randomIndex = math.random(1, buttonCount)

	for i = 1, buttonCount do
		if (i == randomIndex) then
			joypadTable[i] = 1
		else
			joypadTable[i] = 0
		end
	end

	return joypadTable
end

function getRandomOutputFromExplorationExploitation(buttonCount, explorationExploitationValue)
	local joypadTable = {}
	local randomValue = math.random(0.0, 100.0)
	local bestIndex = 1

	joypadTable[1] = 0
	joypadTable[2] = explorationExploitationValue

	for i = 1, buttonCount do
		if i == 1 then
			joypadTable[i] = 0

		elseif i == 2 then
			joypadTable[i] = explorationExploitationValue

			if randomValue >= joypadTable[i] then
				bestIndex = i
			else
				break
			end

		else
			joypadTable[i] = joypadTable[i - 1] + ((100 - joypadTable[i - 1]) / 2)

			if randomValue >= joypadTable[i] then
				bestIndex = i
			else
				break
			end
		end
	end

	for i = 1, buttonCount do
		joypadTable[i] = 0
	end

	joypadTable[bestIndex] = 1

	return joypadTable
end

function getJoypadTableFromOutput(output)
	local joypadTable = {
		Left = false,
		Right = false,
		Up = false,
		Down = false,
		A = false
	}

	local maxAction = getMaxIndexFromTable(output)

	if (maxAction == OutputType.Left) then joypadTable = { Left = true }
	elseif (maxAction == OutputType.Right) then joypadTable = { Right = true }
	elseif (maxAction == OutputType.Up) then joypadTable = { Up = true }
	elseif (maxAction == OutputType.Down) then joypadTable = { Down = true }
	elseif (maxAction == OutputType.A) then joypadTable = { A = true }
	end

	return joypadTable
end

function getOutputFromJoypadTable(joypadTable, buttonCount)
	local output = {}

	if joypadTable["Left"] then output[OutputType.Left] = 1 else output[OutputType.Left] = 0 end
	if joypadTable["Right"] then output[OutputType.Right] = 1 else output[OutputType.Right] = 0 end
	if joypadTable["Up"] then output[OutputType.Up] = 1 else output[OutputType.Up] = 0 end
	if joypadTable["Down"] then output[OutputType.Down] = 1 else output[OutputType.Down] = 0 end
	if joypadTable["A"] then output[OutputType.A] = 1 else output[OutputType.A] = 0 end

	return output
end

function updateOutputOnJoypadControl()
	if not joypadControl then
		return
	end

	if not isTableEqual(input.value, input.previousValue) then
		local outputTemp = getOutputFromJoypadTable(joypad.getimmediate(), getOutputSize())
		local isAllOutputNegative = true

		for i = 1, #outputTemp do
			if outputTemp[i] == 1 then
				isAllOutputNegative = false
				break
			end
		end

		if not isAllOutputNegative then
			output = outputTemp
			updatePreviousOutput()
		end
	end
end

function updateJoypadInput()
	if joypadControl then
		return
	end

	-- Make sure to not keep holding action keys down
	if joypadTable["A"] and isActionKeyPressed then
		joypadTable["A"] = false
	else
		isActionKeyPressed = false
	end

	setJoypadInput(joypadTable)

	if joypadTable["A"] then
		isActionKeyPressed = true
	end
end

function updateRunTimer()
	if not isRunTimerActive then
		return
	end

	runTimer = runTimer + 1

	if runTimer >= runTime then
		if debugMode then
			print("Next run due to timer ending")
		end

		return false
	end

	return true
end

function updateStuckTimer()
	if not isStuckTimerActive then
		return true
	end

	stuckTimer = stuckTimer + 1

	if stuckTimer >= stuckTime then
		stuckTimer = 0

		if debugMode then
			print("Next run due to being stuck")
		end

		return false
	end

	return true
end

function enableRunTimers()
	isRunTimerActive = true
	isStuckTimerActive = true
end

function getColorFromTileType(tileType)
	local color

	if tileType == TileType.Unexplored then
		color = TileTypeColor.Unexplored
	elseif tileType == TileType.Explored then
		color = TileTypeColor.Explored
	elseif tileType == TileType.Wall then
		color = TileTypeColor.Wall
	elseif tileType == TileType.ObjectEvent then
		color = TileTypeColor.ObjectEvent
	elseif tileType == TileType.Warp then
		color = TileTypeColor.Warp
	elseif tileType == TileType.CoordEvent then
		color = TileTypeColor.CoordEvent
	elseif tileType == TileType.BackgroundEvent then
		color = TileTypeColor.BackgroundEvent
	elseif tileType == TileType.NPC then
		color = TileTypeColor.NPC
	elseif tileType == TileType.Grass then
		color = TileTypeColor.Grass
	elseif tileType == TileType.DialogueFinished then
		color = TileTypeColor.DialogueFinished
	else
		print("The tileType does not have a specified color")

		color = TileTypeColor.Explored
	end

	return color
end

function initInput()
	input = Input:new()
end

function initTrainingLoop()
	-- Start of run initialization
	if isRunTimerActive then
		-- Reset previous player position if a new run started
		updatePlayerPreviousPosition()

		--playerRepetitiveStuckPositionX = playerPositionX
		--playerRepetitiveStuckPositionY = playerPositionY

		-- Start position is explored
		setCurrentPlayerTile(TileType.Explored)
	end
end

function trainingLoopPlayerMoved()
	-- Player moved one step
	if playerMovedOneStep() then
		--[[
		if not updatePlayerRepetitiveStuck() then
			return false
		end
		]]

		explorePlayerTile()

		if playerDistanceMovedCurrentSteps < playerDistanceMovedSteps then
			playerDistanceMovedCurrentSteps = playerDistanceMovedCurrentSteps + 1
		end

		stuckTimer = 0
	end

	return true
end

function nextRun()
	stuckTimer = 0
	runTimer = 0

	input:reset()

	output = {}
	previousOutput = {}

	if Analytics["CurrentCorrectSteps"] > Analytics["BestCorrectSteps"] then
		Analytics["BestCorrectSteps"] = Analytics["CurrentCorrectSteps"]
	end

	Analytics["CurrentCorrectSteps"] = 0

	removeExploredTilesFromWorld()

	-- Only reset at the end of a run so the AI can finish the previous run
	-- Reset at a high value because tanh will take too long to reach 100
	if explorationExploitationTanh >= explorationExploitationTanhMax then
		explorationExploitationLinear = 0
		explorationExploitationTanh = 0
	end

	loadRandomSaveStateIfActive()

	updateFromMemory()

	updateCurrentMapTile()

	playerDistanceMovedCurrentSteps = 0
	playerDistanceMoved = 0
	playerDistanceMovedStartX = playerPositionX
	playerDistanceMovedStartY = playerPositionY

	trainingLoopQLearning()
end

function debugPause(unpauseOnContinue)
	unpauseOnContinue = unpauseOnContinue == nil and true or unpauseOnContinue

	isDebugPauseActive = true

	print("Paused due to a debug pause being called")

	client.pause()

	while isDebugPauseActive do
		emu.yield()
	end

	if unpauseOnContinue then
		client.unpause()
	end
end

-- TRAINING SERVER METHODS

function connectToTrainingServer()
	if offlineMode then
		return
	end

	trainingClient = socket.tcp()

	local connected, error = trainingClient:connect(trainingServerAddress, trainingServerPort)

	if not connected then
		print("Failed to connect to Training Server: ", error)

		return false
	end

	print("Connected to Training Server")

	return true
end

function sendToTrainingServer(message, reconnectOnFail)
	reconnectOnFail = reconnectOnFail or false

	if offlineMode then
		return
	end

	local messageSent, error = trainingClient:send(message)

	if not messageSent then
		if reconnectOnFail then
			connectToTrainingServer()
		end

		print("Failed to send to Training Server: " .. error)
	end
end

function getMessageToSend(...)
	local message = ""
	local argumentCount = select("#", ...)

	for i, value in ipairs({...}) do
		local messagePart

		if type(value) == "table" then
			messagePart = table.concat(value, ",")
		else
			messagePart = tostring(value)
		end

		-- Make sure that the last argument does not have a comma
		if i < argumentCount then
			message = message .. messagePart .. ","
		else
			message = message .. messagePart
		end
    end

	message = message .. "<EOF>"

	return message
end

function receiveOutputFromTrainingServer()
	if offlineMode then
		return getRandomOutput(getOutputSize())
	end

	local receiveAttempt = 1
	local output = nil

	while true do
		local data, error, partial = trainingClient:receive("*l")

		if data then
			output = {}

			data = data:gsub("<EOF>", "")

			for value in data:gmatch("[^,]+") do
				table.insert(output, tonumber(value))
			end

			break
		end

		print("Failed to receive from Training Server (Attempt " .. receiveAttempt .. "): " .. error)

		if receiveAttempt <= maxReconnectAttempts then
			break
		end

		receiveAttempt = receiveAttempt + 1
	end

	if not output then
		output = getRandomOutput(getOutputSize())
	end

	return output
end

-- START METHODS

function runQLearningTraining()
	randomSeed()

	console.clear()

	memory.usememorydomain("Combined WRAM")

	loadRandomSaveStateIfActive()

	if not connectToTrainingServer() then
		offlineMode = true
	end

	updateFromMemory()

	initInput()

	--loadWorld(worldGridSaveFileName)
	updateCurrentMapTile()

	enableRunTimers()

	joypadControl = false

	updatePlayerPreviousPosition()

	playerDistanceMovedStartX = playerPositionX
	playerDistanceMovedStartY = playerPositionY

	trainingLoopQLearning()
end

-- TRAINING LOOPS

function trainingLoopQLearning()
	initTrainingLoop()

	while true do
		updateFromMemory()

		currentReward = 0

		updateTilesOnCurrentMapTile()

		if not trainingLoopPlayerMoved() then
			nextRun()

			break
		end

		updateInput()

		updateRewardForDialogue()

		updateRewardForDistance()

		updateOutputOnJoypadControl()

		updateExplorationExploitation()

		updateJoypadInput()

		if not updateRunTimer() then
			nextRun()

			break
		end

		updateSavingReplay()

		if not updateStuckTimer() then
			saveReplayWalkingAgainstWall()

			nextRun()

			break
		end

		emu.frameadvance()

		updateOutputResetOnPlayerRotationBug()

		updatePlayerPreviousPosition()

		updatePreviousOutput()
	end
end

-- CLASSES

function Input:new()
	local this = {}
	setmetatable(this, self)
	self.__index = self

	this.value = {}
	this.previousValue = {}
	this.inputFromPlayerMapTile = {}
	this.previousInputFromPlayerMapTile = {}
	this.inputFromPlayerInDialogueType = DialogueType.None
	this.previousInputFromPlayerInDialogueType = DialogueType.None
	this.inputFromPlayerAbleToGoNextDialogue = false
	this.previousInputFromPlayerAbleToGoNextDialogue = false
	this.inputFromPlayerRotation = nil
	this.inputFromPlayerDistanceMoved = nil

	return this
end

function Input:reset()
	self.value = {}
	self.previousValue = {}
	self.inputFromPlayerMapTile = {}
	self.inputFromPlayerInDialogueType = DialogueType.None
	self.previousInputFromPlayerInDialogueType = DialogueType.None
	self.inputFromPlayerAbleToGoNextDialogue = false
	self.previousInputFromPlayerAbleToGoNextDialogue = false
	self.inputFromPlayerRotation = nil
	self.inputFromPlayerDistanceMoved = nil
end

function Input:update()
	copyTable(self.value, self.previousValue)

	if self.inputFromPlayerMapTile then
		copyTable(self.inputFromPlayerMapTile, self.previousInputFromPlayerMapTile)
	end

	self.previousInputFromPlayerInDialogueType = self.inputFromPlayerInDialogueType
	self.previousInputFromPlayerAbleToGoNextDialogue = self.inputFromPlayerAbleToGoNextDialogue

	self.inputFromPlayerMapTile = getNeuralNetworkInputFromPlayerMapTile()
	self.inputFromPlayerInDialogueType = getNeuralNetworkInputFromPlayerInDialogueType()
	self.inputFromPlayerAbleToGoNextDialogue = getNeuralNetworkInputFromPlayerAbleToGoNextDialogue()
	self.inputFromPlayerRotation = getNeuralNetworkInputFromPlayerRotation()
	self.inputFromPlayerDistanceMoved = getNeuralNetworkInputFromPlayerDistanceMoved()

	self.value = self:calculateValue()
end

function Input:calculateValue()
	local result = {}

	local inputFromPlayerMapTileNormalized = self:getNormalizedTableFromEnum(TileType, self.inputFromPlayerMapTile)
	local inputFromPlayerInDialogueTypeNormalized = self:getNormalizedValueFromEnum(DialogueType, self.inputFromPlayerInDialogueType)
	local inputFromPlayerRotationNormalized = self:getNormalizedValueFromEnum(RotationType, self.inputFromPlayerRotation)

	-- Insert all seperate input tables into one table
	for _, value in ipairs(inputFromPlayerMapTileNormalized) do
		table.insert(result, value)
	end

	table.insert(result, inputFromPlayerInDialogueTypeNormalized)
	table.insert(result, boolToNumber(self.inputFromPlayerAbleToGoNextDialogue))
	table.insert(result, inputFromPlayerRotationNormalized)
	table.insert(result, self.inputFromPlayerDistanceMoved)

	return result
end

function Input:getNormalizedTableFromEnum(enum, enumInstance)
	local normalizedTable = {}

	for i = 1, #enumInstance do
		normalizedTable[i] = self:getNormalizedValueFromEnum(enum, enumInstance[i])
	end

	return normalizedTable
end

function Input:getNormalizedValueFromEnum(enum, enumInstance)
	local result = enumInstance / (getEnumSize(enum) - 1)

	result = string.format("%.2f", result)

	return tonumber(result)
end

function Input:print()
	print(self.value)
end

-- START PROGRAM

runQLearningTraining()