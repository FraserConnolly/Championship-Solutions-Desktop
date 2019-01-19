from Helpers import printChampionshipName, RemoveResult
cont = {}
cont [ 'Championship' ] = printChampionshipName(CS)

if 'rank' in locals():
    if 'eventid' in locals():
        RemoveResult(CS, eventid, rank)
        Server.Redirect("resEvent.py?id=" + eventid)
print ( 'Error' )
