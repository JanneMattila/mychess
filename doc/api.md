# API

## Endpoints

GET `https://{host}/api/games`

- Returns array of **active** games
- Fields limited to header level
  - `"id"`
  - `"title"`
  - `"opponent"`
  - `"updated"`

GET `https://{host}/api/archive`

- Returns array of archive games
- Fields limited to header level

GET `https://{host}/api/games/123-abc-123`

- Returns full single game content
