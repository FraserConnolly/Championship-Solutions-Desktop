from Helpers import printChampionshipName
cont = {}


available = True
transport = Availibility
if Availibility == "No" :
    available = False
    transport = None

CS.SetAvailability ( int ( athleteid ) , championship, available, transport, PreferredEvent, PersonalBest)

cont [ 'NextPage' ] = "wizardPage3.py"
cont [ 'AthleteID' ] = athleteid

print Server.UseNDjando("wizardpage2.html",cont);
