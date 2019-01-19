from Helpers import printChampionshipName
cont = {}


key = "ESAA_Registration"
note = ESAA

CS.SetNote ( int ( athleteid ), key, note)

cont [ 'NextPage' ] = "wizardPage6.py"
cont [ 'AthleteID' ] = athleteid
cont [ 'Athlete' ] = CS.getAthlete(int(athleteid))
cont [ 'PhoneNumbers' ] =  [ x for x in cont [ 'Athlete' ].Contacts if x.Discriminator == "PhoneContactDetail" or  x.Discriminator == "MobileContactDetail" ] 

print Server.UseNDjando("wizardpage5.html",cont);
