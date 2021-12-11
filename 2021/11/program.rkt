#lang racket

(define (createWorld lines)
  (flatten
    (for/list ([line lines])
      (for/list ([c (string->list line)])
        (- (char->integer c) 48)))))

(define world (createWorld (file->lines "input.txt")))
(define width 10)
(define iterations 100)
(define flashcount 0)

(define neighbors (list (- -1 width) (- 0 width) (- 1 width) -1 1 (+ -1 width) width (+ 1 width) ))

(define (advance world)
  (define stack (build-list 100 values))
  (define flashed empty)

  (define (push x)
    (set! stack (cons x stack)) )

  (define (pop)
    (define result (first stack))
    (set! stack (rest stack))
    result )

  (define (in-scope current new)
    (and
      (>= new 0)
      (< new (length world))
      (<= (abs (- (modulo current width) (modulo new width))) 1) ) )

  (define index empty)
  (do ()
    [(empty? stack)]

    (set! index (pop))
    (cond
      [ (not (equal? 10 (list-ref world index)))

        (define v (+ 1 (list-ref world index)))
        (cond
          [ (< v 10) (set! world (list-set world index v)) ]
          [ (eq? v 10)

            (cond [
              (not (member index flashed))

              (for ([i
                (filter (λ(e) ( in-scope index (+ e index) )) neighbors)
              ]) (push (+ i index)) )
              (set! flashed (cons index flashed))
              (set! flashcount (+ flashcount 1))
            ])

            (set! world (list-set world index 10))
          ] )
      ] ) )

  (set! world (map (λ(e) (modulo e 10)) world ))
  world )

(define (displayWorld world width)
  (cond [ (not (empty? world))

    (displayln (take world width))
    (displayWorld (drop world width) width)
  ]) )

(define lastflashcount 0)
(define i 0)
(do ()
  [ (eq? flashcount (+ (* width width) lastflashcount)) ]

  (cond [ (eq? i 100) (displayln (~a "Step 1: " flashcount )) ])

  (set! lastflashcount flashcount)
  (set! world (advance world))
  (set! i (+ i 1)) )

(displayln (~a "Step 2: " i ))
