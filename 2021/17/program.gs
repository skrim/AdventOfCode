init
	var x1 = 175
	var x2 = 227
	var y1 = -134
	var y2 = -79

	var hitCount = 0

	for var vx = 1 to (x2 + 1)
		for var vy = (y1 + 1) to (-y1 + 1)
			var hit = false
			var tx = 0
			var ty = 0

			var n = 0
			while true
				n++
				if n < vx
					tx += vx - n

				ty += vy - n

				if tx > x2 || ty < y1
					break

				if tx >= x1 && ty <= y2
					hit = true
					break

			if hit
				hitCount++

	print "Part 1: %d", y1 * (y1 + 1) / 2
	print "Part 2: %d", hitCount
