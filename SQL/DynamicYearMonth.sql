USE [TMS_PD_OM]
GO

/****** Object:  StoredProcedure [dbo].[usp_Report_ProjectManagerReport]    Script Date: 2019/1/8 17:57:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




ALTER procedure [dbo].[usp_Report_ProjectManagerReport] 
@StartTime varchar(20),
@EndTime varchar(20),
@CalendarType varchar(20),
@PMID int,
@ProjectStatus varchar(20)
as
begin
	declare @sql varchar(8000)
	declare @strProjectCondition varchar(200)
	declare @start date
	declare @end date

	--get earlier hours and actual hours and save them in a temp table
	create table #temp
	 (
		H_PrjID int,
		Prj_PMID int,
		H_UserID int,
		EarlierHours Decimal(8,1),
		ActualHours Decimal(8,1)
	 )

	 create table #tempYearMonth
	 (
		YearMonth varchar(20)
	 )


	if(@CalendarType='System')
	begin
		select @start=dateadd(month,1,dateadd(day,-20,@StartTime))
		select @end=dateadd(month,1,dateadd(day,-20,@EndTime))
	end
	else
	begin
		select @start=@StartTime
		select @end=@EndTime
	end

	;WITH cte AS 
	(
		SELECT dt = DATEADD(DAY, -(DAY(@start) - 1), @start)

		UNION ALL

		SELECT DATEADD(MONTH, 1, dt)
		FROM cte
		WHERE dt < DATEADD(DAY, -(DAY(@end) - 1), @end)
	)
	insert into #tempYearMonth
	select convert(varchar(7),dt,121)
	--select convert(varchar(7),dateadd(day,-20,dt),121),dt
	--select convert(varchar(7),dateadd(month,1,dateadd(day,-20,dt)),121),dt
	FROM cte


	

	 insert into #temp
	 select t.h_prjid,t.prj_pmid,t.h_userid,sum(Earlier_Hours) as EarlierHours,sum(Actual_Hours) as ActualHours 
	from 
	(select h.H_PrjID,p.Prj_PMID,h.H_UserID,Sum(h.h_hours) as Earlier_Hours,0 as Actual_Hours from tbl_Hours h 
	join tbl_Project p on h.H_PrjID=p.Prj_ID
	where h.H_Status='Approved' and 
	h.H_Date>=convert(varchar(50),p.[Prj_CreatedTime],101) 
	and h.H_Date<convert(varchar(50),@StartTime,101)
	and p.Prj_PMID=@PMID
	GROUP BY H_PrjID,p.Prj_PMID,H_UserID

	union all
	select h.H_PrjID,p.Prj_PMID,h.H_UserID,0 as Earlier_Hours, Sum(h.h_hours) as Actual_Hours from tbl_Hours h 
	join tbl_Project p on h.H_PrjID=p.Prj_ID
	where h.H_Status='Approved' and 
	h.H_Date>=convert(varchar(50),p.[Prj_CreatedTime],101) 
	and h.H_Date<convert(varchar(50),@EndTime,101)
	and p.Prj_PMID=@PMID
	GROUP BY H_PrjID,p.Prj_PMID,H_UserID)t
	GROUP BY H_PrjID,Prj_PMID,H_UserID

	--build the query to get monthly hours
	if(@ProjectStatus='Active')
	begin
		set @strProjectCondition=' and p.Prj_Status=1 '
	end
	else if(@ProjectStatus='Inactive')
	begin 
		set @strProjectCondition=' and p.Prj_Status=0 '
	end
	else
	begin
		set @strProjectCondition=' and 1=1 '
	end


	--set @sql = 'SELECT p.Prj_Name as [Project Name],u.User_Name as [Team Member],p.Prj_CreatedTime as [WBS Created Date],p.Prj_PMID,p.Prj_PlanHours,temp.EarlierHours,temp.ActualHours,mh.* from (select H_PrjID,H_UserID '
	set @sql = 'SELECT p.Prj_Name as [Project Name],u.User_Name as [Team Member],
	(case p.[Prj_WBSTypeID] when 1 then p.Prj_CreatedTime else null end) as [WBS Created Date],p.Prj_PlanHours as [Plan Hours],
	temp.ActualHours as [Actual Hours],temp.EarlierHours as [Earlier Hours],mh.* from (select H_PrjID,H_UserID '
	if(@CalendarType='System')
	begin
		select @sql = @sql +' , sum(case convert(varchar(7),dateadd(month,1,dateadd(day,-20,H_Date)),121) when ''' + YearMonth + ''' then H_hours else 0 end)['+a.YearMonth+']'
		from
		(select distinct  YearMonth from #tempYearMonth
		) as a order by a.YearMonth
		
	end
	ELSE
	BEGIN
		select @sql = @sql +' , sum(case convert(varchar(7),H_Date,121) when ''' + YearMonth + ''' then H_hours else 0 end)['+a.YearMonth+']'
		from
		(select distinct  YearMonth from #tempYearMonth
		) as a order by a.YearMonth
	END
	set @sql = @sql + ' from tbl_Hours where  h_date between ''' + convert(varchar(10),@StartTime,120) 
	+ ''' and ''' + convert(varchar(10),@EndTime,120) 
	+ ''' group by H_PrjID,H_UserID)mh '
	+' join tbl_Project p on mh.h_prjid=p.prj_id '
	+' join #temp temp on temp.h_prjid=mh.h_prjid and temp.H_UserID=mh.H_UserID and temp.Prj_PMID=p.Prj_PMID'+
	+' join tbl_User u on u.User_ID=mh.H_UserID '
	+ ' where p.prj_pmid='+Convert(varchar(10),@PMID) + @strProjectCondition

	exec(@sql) 

	--drop temp table
	drop table #temp
	drop table #tempYearMonth
end
GO


