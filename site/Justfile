_default:
	@just --list --unsorted --list-heading '' --list-prefix '—— '

# Serve on localhost:1111
serve:
	zola serve --drafts

# Serve on $HOSTNAME.local:4242
serve-network:
	zola serve --drafts -i 0.0.0.0 -p 4242 -u $HOSTNAME.local
