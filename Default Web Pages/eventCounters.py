from Helpers import printChampionshipName
cont = {}
cont [ 'Championship' ] = printChampionshipName(CS)
cont [ 'Events' ] = CS.getCurrentChampionship().getEvents()
cont [ 'Fevents' ] = [ x for x in CS.getCurrentChampionship().getEvents() if x.ShortName.startswith("F")]
cont [ 'Tevents' ] = [ x for x in CS.getCurrentChampionship().getEvents() if x.ShortName.startswith("T")]
print Server.UseNDjando("eventCounters.html",cont)