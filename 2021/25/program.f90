program aoc
    implicit none

    integer, dimension(200, 200) :: current

    character(len=200) :: line
    character(len=1) :: char

    integer :: ios
    integer, parameter :: read_unit = 99
    integer :: width, height
    integer :: x, y
    integer :: iteration

    logical :: changed

    current(:,:) = 0

    open(unit=read_unit, file="input.txt", iostat=ios)

    y = 1

    do
        read(read_unit, '(A)', iostat=ios) line
        if (ios /= 0) exit

        x = 1

        do
            char = line(x:x+1)

            if (char .eq. ">") then
                current(x, y) = 1
            else if (char .eq. "v") then
                current(x, y) = 2
            else if (char .ne. ".") then
                exit
            end if

            x = x + 1
        end do

        width = x - 1
        y = y + 1
    end do

    height = y - 1

    iteration = 0

    do
        changed = .false.

        do y = 1, height
            do x = 1, width
                if (current(x, y) .eq. 1) then
                    if (current(wrap(x + 1, width), y) .eq. 0) then
                        current(x, y) = 8
                        current(wrap(x + 1, width), y) = 3
                        changed = .true.
                    end if
                end if
            end do
        end do

        do y = 1, height
            do x = 1, width
                if (current(x, y) .eq. 2) then
                    if (current(x, wrap(y + 1, height)) .eq. 0 .or. current(x, wrap(y + 1, height)) .eq. 8) then
                        current(x, y) = 9
                        current(x, wrap(y + 1, height)) = 4
                        changed = .true.
                    end if
                end if
            end do
        end do

        do y = 1, height
            do x = 1, width
                if (current(x, y) .eq. 3) then
                    current(x, y) = 1
                end if
                if (current(x, y) .eq. 4) then
                    current(x, y) = 2
                end if
                if (current(x, y) .gt. 7) then
                    current(x, y) = 0
                end if
            end do
        end do

        iteration = iteration + 1

        if (.not.(changed)) then
            exit
        end if
    end do

    print *, "Part 1:", iteration

contains

    pure integer function wrap(v, size) result(wrapped)
        integer, intent(in) :: v, size

        if (v .lt. 1) then
            wrapped = v + size
        else if (v .gt. size) then
            wrapped = v - size
        else
            wrapped = v
        end if
    end function

end program aoc