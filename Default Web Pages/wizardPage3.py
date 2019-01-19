from Helpers import printChampionshipName
cont = {}

available = True
transport = Availibility
if Availibility == "No" :
    available = False
    transport = None

CS.SetAvailability ( int ( athleteid ), championship, available, transport, None, None)

cont [ 'NextPage' ] = "wizardPage4.py"
cont [ 'AthleteID' ] = athleteid

print Server.UseNDjando("wizardpage3.html",cont);
