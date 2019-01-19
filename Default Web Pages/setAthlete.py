from Helpers import printChampionshipName, AddResult
cont = {}
cont [ 'Championship' ] = printChampionshipName(CS)

newAthleteID = CS.SetAthleteName(int(athleteid),name,preferedname)
                    
if newAthleteID == 0:
    print ( 'error saving athlete' )
else:
    Server.Redirect("editAthlete.py?athleteid=" + str(newAthleteID))
print ( 'Error' )
