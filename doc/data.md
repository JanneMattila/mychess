# Data

## JSON schemas

### Player

Here is example game format:

```json
{
  "id": "123-abc-123",
  "name": "John Doe",
  "created": "2020-04-05T15:48:57.484Z",
  "updated": "2020-04-06T15:48:57.484Z"
}
```

### Game

Here is example game format:

```json
{
  "id": "123-abc-123",
  "name": "Example game",
  "created": "2020-04-05T15:48:57.484Z",
  "updated": "2020-04-06T15:48:57.484Z",
  "state": "Normal",
  "stateText": "Normal",
  "players": {
    "white": {
      "id": "123-abc-123",
      "name": "John Doe"
    },
    "black": {
      "id": "123-def-123",
      "name": "Jane Doe"
    }
  },
  "moves": [
    {
      "move": "A1A2",
      "comment": "Here we go!",
      "start": "2020-04-05T15:48:57.484Z",
      "end": "2020-04-05T15:49:05.000Z"
    },
    {
      "move": "G7G6",
      "comment": "I'm ready",
      "start": "2020-04-05T15:50:57.484Z",
      "end": "2020-04-05T15:51:05.000Z"
    }
  ]
}
```

**Note**: Element `players`:

```json
"players": {
  "white": {
    "id": "123-abc-123",
    "name": "John Doe"
  },
  "black": {
    "id": "123-def-123",
    "name": "Jane Doe"
  }
}
```

Will be converted into more approriate data
for the user interface at the backend:

```json
"opponent": "John Doe"
```

### Move

Here is example move format:

```json
{
  "move": "A1A2",
  "comment": "Here we go!",
  "start": "2020-04-05T15:48:57.484Z",
  "end": "2020-04-05T15:49:05.000Z"
}
```

## Table Storage structure

### Player

TBD

### Settings

TBD

### Game

TBD

| PartitionKey | RowKey | Updated |Data* | 
|---|---|---|---|
| user123 | 123-abc-123 | 2020-04-03T15:51:05.000Z | ... |
| user123 | 123-abc-123 | 2020-04-04T15:51:05.000Z | ... |

*) Compressed game data object.
