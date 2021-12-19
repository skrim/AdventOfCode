data class Coordinate(val x: Int, val y: Int, val z: Int) {
    fun add(other: Coordinate) : Coordinate = Coordinate(x + other.x, y + other.y, z + other.z)

    fun subtract(other: Coordinate) : Coordinate = Coordinate(x - other.x, y - other.y, z - other.z)

    fun manhattanDistance(other: Coordinate) : Int = Math.abs(x - other.x) + Math.abs(y - other.y) + Math.abs(z - other.z)

    private fun getHeading(heading: Int) : Coordinate {
        when (heading) { // heading
            0 -> return Coordinate(x, y, z)
            1 -> return Coordinate(-y, x, z)
            2 -> return Coordinate(-x, -y, z)
            3 -> return Coordinate(y, -x, z)
            4 -> return Coordinate(-z, y, x)
            5 -> return Coordinate(z, y, -x)
            else -> throw Exception("Wut")
        }
    }

    private fun getRotation(rotation: Int) : Coordinate {
        when (rotation) { // rotation
            0 -> return Coordinate(x, y, z);
            1 -> return Coordinate(x, -z, y);
            2 -> return Coordinate(x, -y, -z);
            3 -> return Coordinate(x, z, -y);
            else -> throw Exception("Wut")
        }
    }

    fun transform(direction: Int) : Coordinate = getHeading(direction / 4).getRotation(direction % 4)
}

class Scanner() {
    var coordinates: MutableList<Coordinate> = mutableListOf<Coordinate>()
    var position: Coordinate = Coordinate(0, 0, 0)

    fun getRotatedCoordinates(rotation: Int, delta: Coordinate) : MutableList<Coordinate> =
        coordinates.map({ it.transform(rotation).subtract(delta) }).toMutableList()

    fun normalize(rotation: Int, delta: Coordinate) {
        coordinates = getRotatedCoordinates(rotation, delta)
        position = Coordinate(0, 0, 0).subtract(delta)
    }
}

class Program {
    var pendingLocation : MutableList<Scanner> = mutableListOf<Scanner>()
    var pendingCompare : MutableList<Scanner> = mutableListOf<Scanner>()
    var completed : MutableList<Scanner> = mutableListOf<Scanner>()

    fun load() {
        var first = true
        var current : Scanner = Scanner()
        pendingCompare.add(current)

        java.io.File("input.txt").forEachLine {
            if (it.startsWith("---")) {
                if (!first) {
                    current = Scanner()
                    pendingLocation.add(current)
                }
                first = false
            } else if (!it.isNullOrEmpty()) {
                val tokens = it.split(",")
                current.coordinates.add(Coordinate(tokens[0].toInt(), tokens[1].toInt(), tokens[2].toInt()))
            }
        }
    }

    fun iterate() {
        val first = pendingCompare[0]
        var i = 0
        while (i < pendingLocation.count()) {
            val second = pendingLocation[i]
            var found = false
            var fi = 0
            while (!found && fi < first.coordinates.count()) {
                val c1 = first.coordinates[fi++]

                var si = 0
                while (!found && si < second.coordinates.count()) {
                    val c2 = second.coordinates[si++]

                    for (rotation in 0..23) {
                        val tc2 = c2.transform(rotation)
                        val delta = tc2.subtract(c1)
                        var matchAttempt = second.getRotatedCoordinates(rotation, delta)

                        var result = first.coordinates.intersect(matchAttempt)
                        if (result.count() == 12) {
                            second.normalize(rotation, delta)
                            found = true
                        }
                    }
                }
            }
            if (found) {
                pendingLocation.removeAt(i)
                pendingCompare.add(second)
            } else {
                i++
            }
        }
        pendingCompare.removeAt(0)
        completed.add(first)
    }

    fun coordinateCount() : Int = completed.flatMap { it.coordinates }.distinct().count()

    fun maxDistance() : Int {
        var result = 0
        completed.forEach { first ->
            completed.forEach { second ->
                result = Math.max(result, first.position.manhattanDistance(second.position))
            }
        }
        return result
    }

    fun run() {
        load()
        while (pendingLocation.count() + pendingCompare.count() > 0) iterate()
        println("Part 1: ${coordinateCount()}")
        println("Part 2: ${maxDistance()}")
    }
}

fun main() = Program().run()