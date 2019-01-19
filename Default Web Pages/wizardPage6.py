from Helpers import printChampionshipName
cont = {}

CS.AddPhoneNumber ( int ( athleteid) , newNumber )

cont [ 'NextPage' ] = "wizardpage7.py"
cont [ 'AthleteID' ] = athleteid
cont [ 'Athlete' ] = CS.getAthlete(int(athleteid))
cont [ 'Emails' ] =  [ x for x in cont [ 'Athlete' ].Contacts if x.Discriminator == "EmailContactDetail" ] 

print Server.UseNDjando("wizardpage6.html",cont);
