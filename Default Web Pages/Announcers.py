from Helpers import printChampionshipName
cont = {}
cont [ 'Championship' ] = printChampionshipName(CS)

if 'eventcode' in locals():
    cont [ 'Event' ] = CS.getCurrentChampionship().getEventShortName( eventcode )
    cont [ 'Results' ] = cont [ 'Event' ].getResults()
else:
    cont [ 'Event' ] = None
    cont [ 'Results' ] = None

#Event.Championship.getOverallSores("Minor Boys").Where(c => c.ScoringTeamName == "A" && c.Team == CTR.Team).Select(c => c.Points).First() + "\n\n";

overall = CS.getOverallResults()

cont [ 'MBKEN' ] = [ x for x in overall if x.Description == "Minor Boys" and x.Team.Name == "Kennet" ] [0]
cont [ 'MGKEN' ] = [ x for x in overall if x.Description == "Minor Girls" and x.Team.Name == "Kennet" ] [0]

cont [ 'MBNW' ] = [ x for x in overall if x.Description == "Minor Boys" and x.Team.Name == "North Wiltshire" ] [0]
cont [ 'MGNW' ] = [ x for x in overall if x.Description == "Minor Girls" and x.Team.Name == "North Wiltshire" ] [0]

cont [ 'MBSAL' ] = [ x for x in overall if x.Description == "Minor Boys" and x.Team.Name == "Salisbury" ] [0]
cont [ 'MGSAL' ] = [ x for x in overall if x.Description == "Minor Girls" and x.Team.Name == "Salisbury" ] [0]

cont [ 'MBSWI' ] = [ x for x in overall if x.Description == "Minor Boys" and x.Team.Name == "Swindon" ] [0]
cont [ 'MGSWI' ] = [ x for x in overall if x.Description == "Minor Girls" and x.Team.Name == "Swindon" ] [0]

cont [ 'MBWW' ] = [ x for x in overall if x.Description == "Minor Boys" and x.Team.Name == "West Wiltshire" ] [0]
cont [ 'MGWW' ] = [ x for x in overall if x.Description == "Minor Girls" and x.Team.Name == "West Wiltshire" ] [0]

cont [ 'GKEN' ] = cont [ 'MGKEN' ].Points 
cont [ 'GNW'  ] = cont [ 'MGNW'  ].Points 
cont [ 'GSAL' ] = cont [ 'MGSAL' ].Points 
cont [ 'GSWI' ] = cont [ 'MGSWI' ].Points 
cont [ 'GWW'  ] = cont [ 'MGWW'  ].Points 

cont [ 'BKEN' ] = cont [ 'MBKEN' ].Points
cont [ 'BNW'  ] = cont [ 'MBNW'  ].Points
cont [ 'BSAL' ] = cont [ 'MBSAL' ].Points
cont [ 'BSWI' ] = cont [ 'MBSWI' ].Points
cont [ 'BWW'  ] = cont [ 'MBWW'  ].Points

cont [ 'KEN' ] = cont [ 'BKEN' ] + cont [ 'GKEN' ] 
cont [ 'NW'  ] = cont [ 'BNW'  ] + cont [ 'GNW'  ] 
cont [ 'SAL' ] = cont [ 'BSAL' ] + cont [ 'GSAL' ] 
cont [ 'SWI' ] = cont [ 'BSWI' ] + cont [ 'GSWI' ] 
cont [ 'WW'  ] = cont [ 'BWW'  ] + cont [ 'GWW'  ]

print Server.UseNDjando("Announcers.html",cont);
