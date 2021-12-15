with Ada.Text_IO;
use Ada.Text_IO;
with Ada.Strings.Unbounded;
use Ada.Strings.Unbounded;

procedure Program is
    type WorldData is array(0..499, 0..499) of Integer;

    InputFile: file_type;
    sline : unbounded_string;
    fname : unbounded_string;
    xp, yp : Integer;
    size, expansion : Integer;
    line : unbounded_string;
    cost : Integer;

    smallWorld : WorldData;
    largeWorld : WorldData;

    function FindPath (data : in WorldData; size : in Integer) return Integer is
      world: array(0..249999) of Integer;
      previous: array(0..249999) of Integer;
      distances: array(0..249999) of Integer;
      included: array(0..249999) of Boolean;
      nodeCount, processed, minDistance, u, cost : Integer;

      procedure FindDistance (v : in Integer) is
        alt: Integer;
      begin
        if included(v) then
          return;
        end if;

        alt := distances(u) + world (v);
        if alt < distances(v) then
          distances(v) := alt;
          previous(v) := u;
        end if;

      end FindDistance;
    begin
      for x in 0 .. size - 1 loop
        for y in 0 .. size - 1 loop
          world( x + y * size ) := data(x, y);
        end loop;
      end loop;
      nodeCount := size * size;

      for i in 0 .. nodeCount - 1 loop
        distances(i) := 999999;
        previous(i) := -1;
        included(i) := false;
      end loop;

      distances(0) := 0;
      processed := 0;

      while processed < nodeCount loop
        minDistance := 999999;
        u := -1;

        -- this is horribly slow!
        for i in 0 .. nodeCount - 1 loop
          if not included(i) and minDistance > distances(i) then
            minDistance := distances(i);
            u := i;
          end if;
        end loop;

        if u = nodeCount - 1 then
          processed := nodeCount;
        else
          included(u) := true;
          processed := processed + 1;

          if u mod size > 0 then
            FindDistance(u - 1);
          end if;

          if u mod size < size - 1 then
            FindDistance(u + 1);
          end if;

          if u > size then
            FindDistance(u - size);
          end if;

          if u < nodeCount - size then
            FindDistance(u + size);
          end if;
        end if;
      end loop;

      u := nodeCount - 1;
      cost := 0;
      while u > 0 loop
        cost := cost + world(u);
        u := previous(u);
      end loop;

      return cost;

    end FindPath;

begin
    Open(File => InputFile, Mode => In_File, Name => "input.txt");

    expansion := 5;

    yp := 0;
    while not End_Of_File (InputFile) loop

      line := To_Unbounded_String(Get_Line(InputFile));
      size := Length(line);

      xp := 0;
      for i in 1 .. size loop
        smallWorld(xp, yp) := Integer'Value(To_String(line)(i..i));
        xp := xp + 1;
      end loop;

      yp := yp + 1;
    end loop;

    Close(InputFile);

    for yp in 0 .. size * expansion - 1 loop
      for xp in 0 .. size * expansion - 1 loop
        largeWorld(xp, yp) := ((smallWorld(xp mod size, yp mod size) + xp / size + yp / size + 8) mod 9) + 1;
      end loop;
    end loop;

    cost := FindPath(smallWorld, size);
    Put_Line("Part 1: " & Integer'Image(cost));

    cost := FindPath(largeWorld, size * expansion);
    Put_Line("Part 2: " & Integer'Image(cost));
end Program;
