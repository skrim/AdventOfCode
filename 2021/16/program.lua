function createBitReader(input)
    local state = { charIndex = 0, bitIndex = 0, halfbyte = 0 }

    local readBit = function()
        state.bitIndex = state.bitIndex - 1
        if state.bitIndex < 0 then
            state.bitIndex = 3
            state.charIndex = state.charIndex + 1
            state.halfbyte = tonumber(input:sub(state.charIndex, state.charIndex), 16)
        end
        return (state.halfbyte & (1 << state.bitIndex)) >> state.bitIndex;
    end

    return {
        readInteger = function(count)
            local value = 0
            for i = 1, count do
                value = value | (readBit() << count - i)
            end
            return value
        end,

        getPosition = function()
            return state.charIndex * 4 + (3 - state.bitIndex)
        end
    }
end

function createPacketReader(bitreader)
    local result = {}
    local versionSum = 0
    local operations = {
        function(a, b) return a + b end,
        function(a, b) return a * b end,
        math.min,
        math.max,
        nil,
        function(a, b) return a > b and 1 or 0 end,
        function(a, b) return a < b and 1 or 0 end,
        function(a, b) return a == b and 1 or 0 end
    }

    local readSubpackets = function()
        local subpackets = {}
        if bitreader.readInteger(1) == 0 then
            local subpacketLength = bitreader.readInteger(15)
            local bitsRead = 0
            repeat
                local packet = result.readPacket()
                bitsRead = bitsRead + packet.bitsRead
                table.insert(subpackets, packet.value)
            until bitsRead == subpacketLength
        else
            for i = 1, bitreader.readInteger(11) do
                table.insert(subpackets, result.readPacket().value)
            end
        end
        return subpackets
    end

    result.readPacket = function()
        local startPosition = bitreader.getPosition()
        versionSum = versionSum + bitreader.readInteger(3)
        local packetType = bitreader.readInteger(3)
    
        local value = 0
        if packetType == 4 then
            repeat
                local continue = bitreader.readInteger(1)
                value = (value << 4) | bitreader.readInteger(4)
            until continue == 0
        else
            local subpackets = readSubpackets()
            value = subpackets[1]
            for i = 2, #subpackets do 
                value = operations[packetType + 1](value, subpackets[i])
            end
        end
        return { value = value, bitsRead = bitreader.getPosition() - startPosition }
    end

    result.getVersionSum = function() 
        return versionSum
    end

    return result
end

local reader = createPacketReader(createBitReader(io.lines("input.txt")()))
local packet = reader.readPacket()
print ("Part 1: " .. reader.getVersionSum())
print ("Part 2: " .. packet.value)