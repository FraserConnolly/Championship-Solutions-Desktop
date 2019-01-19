if 'competitorid' in locals():
    CS.Promote( int ( competitorid ) )
    Server.Redirect("resEvent.py?id=" + eventid)
print ( 'Error' )
