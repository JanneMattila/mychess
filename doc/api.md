# API

## Endpoints

### Common filters

Each endpoint accepts following filters:

- `skip=n`: Ignore first *n* records
- `take=n`: Take *n* records

### Games

GET `https://{host}/api/games`

- Returns array of **all** users games
- Fields limited to header level
  - `"id"`
  - `"name"`
  - `"opponent"`
  - `"updated"`

GET `https://{host}/api/games?state=WaitingForYou`

- Returns array of games waiting for player input
- Fields limited to header level

GET `https://{host}/api/games?state=WaitingForOpponent`

- Returns array of games waiting for opponent's input
- Fields limited to header level

GET `https://{host}/api/games?state=Archive`

- Returns array of archive games
- Fields limited to header level

GET `https://{host}/api/games/123-abc-123`

- Returns full single game content

POST `https://{host}/api/games`

- Creates new game

POST `https://{host}/api/games/123-abc-123/moves`

- Creates new move on game

### Friends

GET `https://{host}/api/users/me/friends`

- Returns array of all users friends

POST `https://{host}/api/users/me/friends/123-def-123`

- Add user as friend. Optionally overwriting default name.

DELETE `https://{host}/api/users/me/friends/123-abc-123`

- Remove user from friends

### Settings

GET `https://{host}/api/users/me/settings`

- Return players settings

POST `https://{host}/api/users/me/settings`

- Update players settings

### Users

GET `https://{host}/api/users/me`

- Return current user's identifier

POST `https://{host}/api/users/me`

- Return current user's data specified by body

DELETE `https://{host}/api/users/me`

- Delete current user's *all* data
