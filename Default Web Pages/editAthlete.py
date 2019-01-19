from Helpers import printChampionshipName
cont = {}
cont [ 'Championship' ] = printChampionshipName(CS)
if 'athleteid' in locals():
    cont [ 'Title' ] = 'Edit Athlete'
    cont [ 'Athlete' ] = CS.getAthlete(int(athleteid))
    cont [ 'AthID' ] = athleteid
    cont [ 'AthName' ] = cont [ 'Athlete' ].Fullname
    cont [ 'AthDoB' ] = cont [ 'Athlete' ].PrintDoBShort()
    if cont [ 'Athlete' ].Attends != None:
        cont [ 'AthSchool' ] = cont [ 'Athlete' ].Attends.Name
    else:
        cont [ 'AthSchool' ] = ""
    cont [ 'AthGender' ] = cont [ 'Athlete' ].Gender
    cont [ 'TeamID' ] = cont [ 'Athlete' ].getTeam(CS.getCurrentChampionship()).ID
    cont [ 'Schools' ] = CS.getSchools( cont [ 'TeamID' ] )
    cont [ 'EligibleEvents' ] = CS.getCurrentChampionship().listAllAvailableEvents(CS.getAthlete(int(athleteid)));
    cont [ 'EnteredEvents' ] = cont [ 'Athlete' ].CompetingAs( CS.getCurrentChampionship() );
    cont [ 'Results' ] = cont [ 'Athlete' ].getAllResults()
    #cont [ 'CertFiles' ] = CS.OpenCert( int ( athleteid ) )
    cont [ 'Contacts' ] =  cont [ 'Athlete' ].Contacts #[ x for x in cont [ 'Athlete' ].Contacts if x.Discriminator == "PhoneContactDetail" or  x.Discriminator == "MobileContactDetail" ] 
    print Server.UseNDjando("editAthlete.html",cont);        
#else:
#	if 'teamid' in locals():
#		cont [ 'Title' ] = 'Add new Athlete'
#		cont [ 'Schools' ] = CS.getSchools(int(teamid))
#		cont [ 'TeamID' ] = teamid
#		cont [ 'AthID' ] = 0
#        #Added AthID = 0 in V3-0 2016-04-24
else:
	Server.Redirect("error.html")
	cont [ 'AthName' ] = ''
	cont [ 'AthDoB' ] = ''
	cont [ 'AthID' ] = 0
	cont [ 'AthGender' ] = ''

#print Server.UseNDjando("editAthlete.html",cont);    

