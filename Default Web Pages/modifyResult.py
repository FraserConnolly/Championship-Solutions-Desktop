from Helpers import printChampionshipName, AddResult
cont = {}
cont [ 'Championship' ] = printChampionshipName(CS)

if 'rank' in locals():
    if 'vest' in locals():
        if 'value' in locals():
            if 'eventid' in locals():
                AddResult(CS, eventid, rank, vest, value)
                Server.Redirect("resEvent.py?id=" + eventid)
print ( 'Error' )
