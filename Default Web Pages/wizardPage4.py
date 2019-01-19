from Helpers import printChampionshipName
cont = {}

key = "TShirt"
note = Size

CS.SetNote ( int ( athleteid ), key, note)

cont [ 'NextPage' ] = "wizardPage5.py"
cont [ 'AthleteID' ] = athleteid

print Server.UseNDjando("wizardpage4.html",cont);
