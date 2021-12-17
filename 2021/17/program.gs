init
	var x1 = 175
	var x2 = 227
	var y1 = -134
	var y2 = -79

	var maxApogee = -9999999
	var hitCount = 0

	for var vx = 1 to x2
		for var vy = y1 to 500
			var hit = false
			var apogee = -9999999
			var tx = 0
			var ty = 0

			var n = 0
			while true
				n = n + 1
				if (n < vx)
					tx = tx + (vx - n)

				ty = ty + vy - n

				if ty > apogee
					apogee = ty

				if ty < y1 || tx > x2
					break

				if tx >= x1 && tx <= x2 && ty >= y1 && ty <= y2
					hit = true
					break

			if hit 
				hitCount++
				if apogee > maxApogee
					maxApogee = apogee
						
	print "Part 1: %d", maxApogee
	print "Part 2: %d", hitCount
