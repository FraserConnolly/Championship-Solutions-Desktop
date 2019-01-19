def printChampionshipName(CS):
    return CS.getCurrentChampionship().Name

def AddResult(CS, eventid, rank, vest, value):
    CS.AddResult( int (eventid), int(rank), vest, value)

def RemoveResult(CS, eventid, rank):
    CS.RemoveResult( int (eventid), int(rank) )

def SetValue (CS, eventid, rank, value):
    CS.SetValue( int (eventid), int(rank), value)