# API

## Endpoints

### Games

GET `https://{host}/api/games`

- Returns array of **all** users games
- Fields limited to header level
  - `"id"`
  - `"name"`
  - `"opponent"`
  - `"updated"`

GET `https://{host}/api/games?state=waitingForYou`

- Returns array of games waiting for player input
- Fields limited to header level

GET `https://{host}/api/games?state=waitingForOpponent`

- Returns array of games waiting for opponent's input
- Fields limited to header level

GET `https://{host}/api/games?state=archive`

- Returns array of archive games
- Fields limited to header level

GET `https://{host}/api/games/123-abc-123`

- Returns full single game content

POST `https://{host}/api/games`

- Creates new game

POST `https://{host}/api/games/123-abc-123/moves`

- Creates new move on game

### Friends

GET `https://{host}/api/friends`

- Returns array of all users friends

POST `https://{host}/api/friends/123-abc-123`

- Add user as friend. Optionally overwriting default name.

DELETE `https://{host}/api/friends/123-abc-123`

- Remove user from friends

### Settings

GET `https://{host}/api/settings`

- Return players settings

POST `https://{host}/api/settings`

- Update players settings

### Users

GET `https://{host}/api/users`

- Return current users all data

DELETE `https://{host}/api/users`

- Delete current users all data
