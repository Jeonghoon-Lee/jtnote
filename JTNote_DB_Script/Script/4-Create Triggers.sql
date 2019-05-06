/**
 * IPD-16 DOTNET2 - FINAL PROJECT
 *
 * Create JTNotes database
 *	Script Date: May 1, 2019
 *  Developed by: Taylor and Jeonghoon
 */

-- switch to JTNotes database
use JTNotes
;
go


/* Create trigger Dbo.DeleteTagTr */
-- This is not used. just for sample
if OBJECT_ID('Dbo.DeleteTagTr' , 'TR') is not null
	drop trigger Dbo.DeleteTagTr
;
go

create trigger Dbo.DeleteTagTr
on Dbo.Tags
instead of delete
as
begin
	set nocount on;
	delete NT 
	from NoteTag as NT
		inner join deleted as D
		on NT.TagId = D.Id
end
;
go