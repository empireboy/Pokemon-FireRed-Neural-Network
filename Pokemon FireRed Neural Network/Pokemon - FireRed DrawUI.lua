UIDrawer = {}

previousDrawMode = nil

function onExit()
	gui.clearGraphics()
	gui.cleartext()
	client.SetClientExtraPadding(0, 0, 0, 0)
	isExtraBorderActive = false
end

function drawInputsIfActive()
	if drawMode ~= DrawMode.Inputs then
		return
	end

	local tileWidth = math.floor(100 / inputScannerWidth)
	local tileHeight = math.floor(100 / inputScannerHeight)

	UIDrawer.drawInputScannerFullscreen(getNeuralNetworkInputFromPlayerMapTileSection(playerNextPositionX, playerNextPositionY, 15, 11), gameCenterX(), gameCenterY(), inputScannerWidth, inputScannerHeight, 32, 32, "Black", 255/2, 255/3, true)

	UIDrawer.drawInputScanner(input.previousInputFromPlayerMapTile, 16, 16, inputScannerHeight, tileWidth, tileHeight, "Black", 255, false)

	UIDrawer.drawInputScanner(input.inputFromPlayerMapTile, 16 + 110, 16, inputScannerHeight, tileWidth, tileHeight, "Black", 255, false)

	gui.text(16, 100 + 20 * 1, "Player In Dialogue: " .. tostring(input.inputFromPlayerInDialogueType))
	gui.text(16, 100 + 20 * 2, "Can Player Go Next Dialogue: " .. tostring(input.inputFromPlayerAbleToGoNextDialogue))
	gui.text(16, 100 + 20 * 3, "Player Rotation: " .. tostring(input.inputFromPlayerRotation))
	gui.text(16, 100 + 20 * 4, "Is Player Walking Against Wall: " .. tostring(isPlayerWalkingAgainstWall))
	gui.text(16, 100 + 20 * 5, "Player Distance Moved: " .. tostring(input.inputFromPlayerDistanceMoved))
end

function drawInputsFullscreenOnlyIfActive()
	if drawMode ~= DrawMode.InputsFullscreen then
		return
	end

	UIDrawer.drawInputScannerFullscreen(getNeuralNetworkInputFromPlayerMapTileSection(playerNextPositionX, playerNextPositionY, 15, 11), gameCenterX(), gameCenterY(), inputScannerWidth, inputScannerHeight, 32, 32, "Black", 255/2, 255/3, true)
end

function drawOutputsIfActive()
	if drawMode ~= DrawMode.Outputs then
		return
	end

	gui.text(16, 16 + 20 * 1, "Output: ")
	gui.text(16, 16 + 20 * 2, "Left: " .. tostring(output[OutputType.Left]))
	gui.text(16, 16 + 20 * 3, "Right: " .. tostring(output[OutputType.Right]))
	gui.text(16, 16 + 20 * 4, "Up: " .. tostring(output[OutputType.Up]))
	gui.text(16, 16 + 20 * 5, "Down: " .. tostring(output[OutputType.Down]))
	gui.text(16, 16 + 20 * 6, "A: " .. tostring(output[OutputType.A]))

	gui.text(16, 16 + 20 * 8, "Previous Output: ")
	gui.text(16, 16 + 20 * 9, "Left: " .. tostring(previousOutput[OutputType.Left]))
	gui.text(16, 16 + 20 * 10, "Right: " .. tostring(previousOutput[OutputType.Right]))
	gui.text(16, 16 + 20 * 11, "Up: " .. tostring(previousOutput[OutputType.Up]))
	gui.text(16, 16 + 20 * 12, "Down: " .. tostring(previousOutput[OutputType.Down]))
	gui.text(16, 16 + 20 * 13, "A: " .. tostring(previousOutput[OutputType.A]))

	gui.text(16 + 200, 16 + 20 * 1, "Joypad: ")
	gui.text(16 + 200, 16 + 20 * 2, "Left: " .. tostring(joypad.getimmediate()["Left"]))
	gui.text(16 + 200, 16 + 20 * 3, "Right: " .. tostring(joypad.getimmediate()["Right"]))
	gui.text(16 + 200, 16 + 20 * 4, "Up: " .. tostring(joypad.getimmediate()["Up"]))
	gui.text(16 + 200, 16 + 20 * 5, "Down: " .. tostring(joypad.getimmediate()["Down"]))
	gui.text(16 + 200, 16 + 20 * 6, "A: " .. tostring(joypad.getimmediate()["A"]))
end

function drawStatisticsIfActive()
	if drawMode ~= DrawMode.Statistics then
		return
	end

	gui.text(16, 16, "Exploration Exploitation: " .. string.format("%.2f", explorationExploitationTanh))
	gui.text(16, 16 + 20 * 1, "Best Correct Steps: " .. Analytics["BestCorrectSteps"])
	gui.text(16, 16 + 20 * 2, "Current Correct Steps: " .. Analytics["CurrentCorrectSteps"])
end

function updateClientExtraPadding()
	-- Only set client padding if draw mode changed to a mode that needs the extra padding
	if (
		drawMode == DrawMode.Inputs
		or drawMode == DrawMode.Outputs
		or drawMode == DrawMode.Statistics
	)
		and previousDrawMode ~= drawMode
	then
		client.SetClientExtraPadding(extraBorderWidth, 0, 0, 0)
		isExtraBorderActive = true

	elseif previousDrawMode ~= drawMode then
		client.SetClientExtraPadding(0, 0, 0, 0)
		isExtraBorderActive = false
	end
end

function runDrawUI()
	event.onexit(onExit)

	gui.use_surface("client")

	while true do
		gui.clearGraphics()
		gui.cleartext()

		updateClientExtraPadding()

		drawInputsIfActive()

		drawInputsFullscreenOnlyIfActive()

		drawOutputsIfActive()

		drawStatisticsIfActive()

		previousDrawMode = drawMode

		emu.yield()
	end
end

function UIDrawer.drawInputScannerFullscreen(input, positionX, positionY, width, height, tileWidth, tileHeight, outlineColor, alpha, outerAlpha, alignCenter)
	positionX = positionX or 16
	positionY = positionY or 16
	height = height or math.sqrt(#input)
	width = width or math.ceil(#input / height)
	tileWidth = tileWidth or 9
	tileHeight = tileHeight or 9
	alignCenter = alignCenter or false

	local inputScannerIndex = 1
	local color

	local outerHeight = 11
	local outerWidth = 15
	local fullWidth = tileWidth * outerWidth
	local fullHeight = tileHeight * outerHeight
	local innerIndexX = (outerWidth - width) / 2
	local innerIndexY = (outerHeight - height) / 2

	if alignCenter then
		positionX = positionX - fullWidth / 2
		positionY = positionY - fullHeight / 2
	end

	local tileAlpha = alpha

	for i = 1, outerWidth do
		for j = 1, outerHeight do
			if not input[inputScannerIndex] then
				break
			end

			if i > innerIndexX
				and i <= innerIndexX + width
				and j > innerIndexY
				and j <= innerIndexY + height
			then
				tileAlpha = alpha
			else
				tileAlpha = outerAlpha

				-- Make tiles less visible that are explored outside the scanner area. This will make the overlay more clear
				if input[inputScannerIndex] == TileType.Explored then
					tileAlpha = tileAlpha * 0.6
				end
			end

			-- Make tiles less visible that are unexplored. This will make the overlay more clear
			if input[inputScannerIndex] == TileType.Unexplored then
				tileAlpha = tileAlpha * 0
			end

			color = getColorFromTileType(input[inputScannerIndex])
			color = createColorWithAlpha(color, tileAlpha)

			outlineColorFinal = outlineColor or color
			outlineColorFinal = createColorWithAlpha(outlineColorFinal, tileAlpha)

			if tileAlpha ~= 0 then
				gui.drawRectangle(
					positionX + tileWidth * (i - 1),
					positionY + tileHeight * (j - 1),
					tileWidth, tileHeight, outlineColorFinal, color
				)
			end

			inputScannerIndex = inputScannerIndex + 1
		end
	end
end

function UIDrawer.drawInputScanner(input, positionX, positionY, height, tileWidth, tileHeight, outlineColor, alpha, alignCenter)
	positionX = positionX or 16
	positionY = positionY or 16
	height = height or math.sqrt(#input)
	tileWidth = tileWidth or 9
	tileHeight = tileHeight or 9
	alignCenter = alignCenter or false

	local inputScannerIndex = 1
	local color

	local width = math.ceil(#input / height)
	local fullWidth = tileWidth * width
	local fullHeight = tileHeight * height

	if alignCenter then
		positionX = positionX - fullWidth / 2
		positionY = positionY - fullHeight / 2
	end

	for i = 1, width do
		for j = 1, height do
			if not input[inputScannerIndex] then
				break
			end

			color = getColorFromTileType(input[inputScannerIndex])
			color = createColorWithAlpha(color, alpha)

			outlineColorFinal = outlineColor or color

			gui.drawRectangle(
				positionX + tileWidth * (i - 1),
				positionY + tileHeight * (j - 1),
				tileWidth, tileHeight, outlineColorFinal, color
			)

			inputScannerIndex = inputScannerIndex + 1
		end
	end
end

-- START

runDrawUI()