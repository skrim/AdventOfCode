class Node
    def initialize(name : String)
        @name = name
        @connections = [] of Node

        @small = false
        @maxVisits = 1000
        if @name.size < 3 && @name.downcase == @name
            @small = true
            @maxVisits = 1
        end

        @visited = 0
    end

    def addConnection(other : Node)
        if (other.@name != "start" && @name != "end")
            @connections << other
        end
    end

    def visit(d : Int)
        @visited = @visited + d
    end

    def allow()
        @visited < @maxVisits
    end

    def setVisits(visits : Int)
        @maxVisits = visits
    end
end

nodes = {} of String => Node

def getNode(nodes : Hash, name : String)
    if (!nodes.has_key?(name))
        nodes[name] = Node.new name
    end
    nodes[name]
end

File.each_line("input.txt") do | line |
    tokens = line.split("-")

    st = tokens[0]
    en = tokens[1]
    a = getNode(nodes, st)
    b = getNode(nodes, en)

    a.addConnection(b)
    b.addConnection(a)
end

def traverse(current : Node, finalTarget : Node, path : Array, visitCountOnPath : Int)
    completedRoutes = 0
    path << current
    current.visit(1)
    first = true
    current.@connections.each do | target |
        if target == finalTarget
            if visitCountOnPath == 0 || path.count { |p| p.@maxVisits == visitCountOnPath }
                completedRoutes = completedRoutes + 1
            end
            next
        end

        if target.allow()
            completedRoutes = completedRoutes + traverse(target, finalTarget, path, visitCountOnPath)
        end
    end
    path.pop()
    current.visit(-1)

    completedRoutes
end

startNode = nodes["start"]
endNode = nodes["end"]

paths = traverse(startNode, endNode, [] of Node, 0)
puts "Part 1: #{paths}"

nodes.values.each do | node |
    if (node.@small)

        node.setVisits(2)
        c = traverse(startNode, endNode, [] of Node, 2)
        node.setVisits(1)

        paths = paths + c
    end
end

puts "Part 2: #{paths}"
