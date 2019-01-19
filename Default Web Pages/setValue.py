from Helpers import printChampionshipName, RemoveResult, SetValue
cont = {}
cont [ 'Championship' ] = printChampionshipName(CS)

if 'rank' in locals():
    if 'eventid' in locals():
        if 'value' in locals():
            SetValue(CS, eventid, rank, value)
            Server.Redirect("resEvent.py?id=" + eventid)
print ( 'Error' )
