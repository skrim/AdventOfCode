drop table if exists #Initialization;
drop table if exists #Iterations;

declare @Data varchar(1000);
set @Data =
'4,1,3,2,4,3,1,4,4,1,1,1,5,2,4,4,2,1,2,3,4,1,2,4,3,4,5,1,1,3,1,2,1,4,1,1,3,4,1,2,5,1,4,2,2,1,1,1,3,1,'+
'5,3,1,2,1,1,1,1,4,1,1,1,2,2,1,3,1,3,1,3,4,5,1,2,2,1,1,1,4,1,5,1,3,1,3,4,1,3,2,3,4,4,4,3,4,5,1,3,1,3,'+
'5,1,1,1,1,1,2,4,1,2,1,1,1,5,1,1,2,1,3,1,4,2,3,4,4,3,1,1,3,5,3,1,1,5,2,4,1,1,3,5,1,4,3,1,1,4,2,1,1,1,'+
'1,1,1,3,1,1,1,1,1,4,5,1,2,5,3,1,1,3,1,1,1,1,5,1,2,5,1,1,1,1,1,1,3,5,1,3,2,1,1,1,1,1,1,1,4,5,1,1,3,1,'+
'5,1,1,1,1,3,3,1,1,1,4,4,1,1,4,1,2,1,4,4,1,1,3,4,3,5,4,1,1,4,1,3,1,1,5,5,1,2,1,2,1,2,3,1,1,3,1,1,2,1,'+
'1,3,4,3,1,1,3,3,5,1,2,1,4,1,1,2,1,3,1,1,1,1,1,1,1,4,5,5,1,1,1,4,1,1,1,2,1,2,1,3,1,3,1,1,1,1,1,1,1,5';

create table #Iterations (Id int, Age0 bigint, Age1 bigint, Age2 bigint, Age3 bigint, Age4 bigint, Age5 bigint, Age6 bigint, Age7 bigint, Age8 bigint);

select Item.Value into #Initialization from string_split(@Data, ',') Item;

insert into #Iterations values (
	0,
	(select count(*) from #Initialization where Value=0),
	(select count(*) from #Initialization where Value=1),
	(select count(*) from #Initialization where Value=2),
	(select count(*) from #Initialization where Value=3),
	(select count(*) from #Initialization where Value=4),
	(select count(*) from #Initialization where Value=5),
	(select count(*) from #Initialization where Value=6),
	(select count(*) from #Initialization where Value=7),
	(select count(*) from #Initialization where Value=8)
);

declare @i int = 0;

while @i <= 256
begin
   insert into #Iterations values (
     @i + 1,
	 (select Age1 from #Iterations where Id = @i),
	 (select Age2 from #Iterations where Id = @i),
	 (select Age3 from #Iterations where Id = @i),
	 (select Age4 from #Iterations where Id = @i),
	 (select Age5 from #Iterations where Id = @i),
	 (select Age6 from #Iterations where Id = @i),
	 (select Age7 + Age0 from #Iterations where Id = @i),
	 (select Age8 from #Iterations where Id = @i),
	 (select Age0 from #Iterations where Id = @i)
   );
   set @i = @i + 1;
end;

select Id, Age0, Age1, Age2, Age3, Age4, Age5, Age6, Age7, Age8, Age0 + Age1 + Age2 + Age3 + Age4 + Age5 + Age6 + Age7 + Age8 Total from #Iterations where Id = 80 or Id = 256;
