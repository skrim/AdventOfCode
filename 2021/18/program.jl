mutable struct Snailfish
    parent::Union{Snailfish, Nothing}
    depth::Int
    left::Union{Snailfish, Nothing}
    right::Union{Snailfish, Nothing}
    value::Union{Int, Nothing}
end
Base.show(io::IO, z::Snailfish) = if (z.value == nothing) print(io, "[", z.left, ",", z.right, "]") else print(io, z.value) end

hasValue(node::Snailfish) = isa(node.value, Number)

splitCondition(node::Snailfish) = hasValue(node) && node.value >= 10

explodeCondition(node::Snailfish) = node.depth > 4 && !hasValue(node) && hasValue(node.left) && hasValue(node.right)

magnitude(node::Snailfish) = hasValue(node) ? node.value : magnitude(node.left) * 3 + magnitude(node.right) * 2

function parseSnailfish(input::String)
    nesting = 1
    current = root = Snailfish(nothing, nesting, nothing, nothing, nothing)

    for token in collect(m.match for m = eachmatch(r"[\[\]\,\d]", input))
        if token == "["
            nesting = nesting + 1
            current.left = Snailfish(current, nesting, nothing, nothing, nothing)
            current.right = Snailfish(current, nesting, nothing, nothing, nothing)
            current = current.left
        elseif token == ","
            current = current.parent.right
        elseif token == "]"
            nesting = nesting - 1
            current = current != root ? current.parent : current
        else
            current.value = parse(Int, token)
        end
    end
    return root
end

function getDepthFirstList(result::Array, node::Snailfish)
    push!(result, node)
    node.left != nothing && getDepthFirstList(result, node.left)
    node.right != nothing && getDepthFirstList(result, node.right)
    return result
end

getDepthFirstList(node::Snailfish) = getDepthFirstList([], node)

function explode(root::Snailfish)
    flat = getDepthFirstList(root)
    for i = 1 : length(flat)
        node = flat[i]
        !explodeCondition(node) && continue

        for nd in filter(hasValue, reverse(flat[1 : i - 1]))
            nd.value = nd.value + node.left.value
            break
        end
        for nd in filter(hasValue, flat[i + 3 : end])
            nd.value = nd.value + node.right.value
            break
        end
        node.left = node.right = nothing
        node.value = 0
        return true
    end
    return false
end

function splitValue(root::Snailfish)
    for node in filter(splitCondition, getDepthFirstList(root))
        node.left = Snailfish(node, node.depth + 1, nothing, nothing, floor(Int, node.value / 2))
        node.right = Snailfish(node, node.depth + 1, nothing, nothing, ceil(Int, node.value / 2))
        node.value = nothing
        return true
    end
    return false
end

function reduce(node::Snailfish)
    while true
        explode(node) || splitValue(node) || break
    end
end

function add(first::Snailfish, second::Snailfish)
    for n in [getDepthFirstList(first); getDepthFirstList(second)]
        n.depth = n.depth + 1
    end

    result = Snailfish(nothing, 1, first, second, nothing)
    first.parent = result
    second.parent = result
    return result
end

function run()
    lines = readlines("input.txt")

    current = parseSnailfish(lines[1])
    for line in lines[2 : end]
        current = add(current, parseSnailfish(line))
        reduce(current)
    end
    println("Part 1: ", magnitude(current))

    maxMagnitude = 0
    for a = 1 : length(lines), b = 1 : length(lines)
        a == b && continue;

        sum = add(parseSnailfish(lines[a]), parseSnailfish(lines[b]))
        reduce(sum)
        maxMagnitude = max(maxMagnitude, magnitude(sum))
    end
    println("Part 2: ", maxMagnitude)
end

run()