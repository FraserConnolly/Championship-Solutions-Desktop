cont = {}

if 'eventid' in locals():
    cont [ 'CertFiles' ] = CS.OpenCert( int ( eventid ) , "" )
    print Server.UseNDjando("ShowCerts.html",cont);
elif 'athleteid' in locals():
    cont [ 'CertFiles' ] = CS.OpenCert( int ( athleteid ) )
    print Server.UseNDjando("ShowCerts.html",cont);
else :
    print ( 'Error' )
