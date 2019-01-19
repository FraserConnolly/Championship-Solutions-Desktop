from Helpers import printChampionshipName
cont = {}
cont [ 'Championship' ] = printChampionshipName(CS)

if 'id' in locals():
    cont [ 'EventID' ] = int ( id )
    cont [ 'Event' ] = CS.getCurrentChampionship().getEvent( int ( id ) )
    cont [ 'EventName' ] = cont [ 'Event' ].PrintableName
    cont [ 'Results' ] = cont [ 'Event' ].getResults()
    cont [ 'NextResult' ] = cont [ 'Results' ].Count + 1
    #cont [ 'CertFiles' ] = CS.OpenCert( int ( id ) , "" )
    cont [ 'FinalEventID' ] = cont [ 'Event' ].FinalID
    cont [ 'EnteredCompetitors' ] = cont [ 'Event' ].getEnteredCompetitors()
    if cont [ 'Event' ]. getEventType() == "Individual Timed Final School Event" or cont [ 'Event' ]. getEventType() == "Individual Timed Final Event" :
        cont [ 'Heats' ] = cont [ 'Event' ].getHeats()
    elif cont [ 'Event' ]. getEventType() == "Individual Timed Heat Event":
        cont [ 'Heats' ] = { cont [ 'Event' ].Final }
    else :
        cont [ 'Heats' ] = None 
    cont [ 'ScannedFiles' ] = cont [ 'Event' ] . Files

    print Server.UseNDjando("resEvent.html",cont);
elif 'EventCode' in locals():

    cont [ 'Event' ] = CS.getCurrentChampionship().getEventShortName( EventCode )
    cont [ 'EventID' ] = cont [ 'Event' ].ID
    cont [ 'EventName' ] = cont [ 'Event' ].PrintableName
    cont [ 'Results' ] = cont [ 'Event' ].getResults()
    cont [ 'NextResult' ] = cont [ 'Results' ].Count + 1
    #cont [ 'CertFiles' ] = CS.OpenCert( cont [ 'EventID' ] , "" )
    cont [ 'FinalEventID' ] = cont [ 'Event' ].FinalID
    cont [ 'EnteredCompetitors' ] = cont [ 'Event' ].getEnteredCompetitors()
    if cont [ 'Event' ]. getEventType() == "Individual Timed Final School Event" or cont [ 'Event' ]. getEventType() == "Individual Timed Final Event" :
        cont [ 'Heats' ] = cont [ 'Event' ].getHeats()
    elif cont [ 'Event' ]. getEventType() == "Individual Timed Heat Event":
        cont [ 'Heats' ] = { cont [ 'Event' ].Final }
    else :
        cont [ 'Heats' ] = None
    cont [ 'ScannedFiles' ] = cont [ 'Event' ] . Files

    print Server.UseNDjando("resEvent.html",cont);
else:
    Server.Redirect("index.py");
