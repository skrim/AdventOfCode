mutable struct Snailfish
    parent::Union{Snailfish, Nothing}
    depth::Int
    left::Union{Snailfish, Nothing}
    right::Union{Snailfish, Nothing}
    value::Union{Int, Nothing}

    Snailfish(parent, depth; left = nothing, right = nothing, value = nothing) = new(parent, depth, left, right, value)
end

Base.show(io::IO, node::Snailfish) = if (hasValue(node)) print(io, node.value) else print(io, "[", node.left, ",", node.right, "]") end

hasValue(node::Snailfish) = isa(node.value, Number)

splitCondition(node::Snailfish) = hasValue(node) && node.value >= 10
explodeCondition(node::Snailfish) = node.depth > 4 && !hasValue(node) && hasValue(node.left) && hasValue(node.right)

function setContents(node::Snailfish, left::Union{Snailfish, Nothing}, right::Union{Snailfish, Nothing}, value::Union{Int, Nothing})
    node.left, node.right, node.value = left, right, value
end

function parseSnailfish(input::String) :: Snailfish
    nesting = 1
    current = root = Snailfish(nothing, nesting)

    for token in collect(m.match for m = eachmatch(r"[\[\]\,\d]", input))
        if token == "["
            nesting = nesting + 1
            setContents(current, Snailfish(current, nesting), Snailfish(current, nesting), nothing)
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
    root
end

function getDepthFirstList(result::Array{Snailfish}, node::Snailfish) :: Array{Snailfish}
    push!(result, node)
    node.left != nothing && getDepthFirstList(result, node.left)
    node.right != nothing && getDepthFirstList(result, node.right)
    result
end

getDepthFirstList(node::Snailfish) :: Array{Snailfish} = getDepthFirstList(Array{Snailfish, 1}(), node)

function explode(root::Snailfish) :: Bool
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
        setContents(node, nothing, nothing, 0)
        return true
    end
    false
end

function splitValue(root::Snailfish) :: Bool
    createChild(node::Snailfish, value::Int) = Snailfish(node, node.depth + 1; value = value)

    for node in filter(splitCondition, getDepthFirstList(root))
        setContents(node, createChild(node, floor(Int, node.value / 2)), createChild(node, ceil(Int, node.value / 2)), nothing)
        return true
    end
    false
end

function reduce(node::Snailfish)
    while true
        explode(node) || splitValue(node) || break
    end
end

function add(first::Snailfish, second::Snailfish) :: Snailfish
    for n in [getDepthFirstList(first); getDepthFirstList(second)]
        n.depth = n.depth + 1
    end
    first.parent = second.parent = Snailfish(nothing, 1; left = first, right = second)
end

function run()
    magnitude(node::Snailfish) = hasValue(node) ? node.value : magnitude(node.left) * 3 + magnitude(node.right) * 2

    lines = readlines("input.txt")

    current = parseSnailfish(lines[1])
    for line in lines[2 : end]
        current = add(current, parseSnailfish(line))
        reduce(current)
    end
    println("Part 1: ", magnitude(current))

    maxMagnitude = 0
    for a in lines, b in lines
        a == b && continue

        sum = add(parseSnailfish(a), parseSnailfish(b))
        reduce(sum)
        maxMagnitude = max(maxMagnitude, magnitude(sum))
    end
    println("Part 2: ", maxMagnitude)
end

run()