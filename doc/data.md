# Data

## JSON schemas

### Player schema

Here is example game format:

```json
{
  "id": "123-abc-123",
  "name": "John Doe",
  "created": "2020-04-05T15:48:57.484Z",
  "updated": "2020-04-06T15:48:57.484Z"
}
```

### Game schema

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
"players": {
  "black": {
    "name": "Jane Doe"
  }
}
```

### Move schema

Here is example move format:

```json
{
  "move": "A1A2",
  "comment": "Here we go!",
  "start": "2020-04-05T15:48:57.484Z",
  "end": "2020-04-05T15:49:05.000Z"
}
```

In case of promotion then additional `promotion` property will be added:

```json
{
  "move": "A1A2",
  "comment": "Here we go!",
  "promotion": "Queen",
  "start": "2020-04-05T15:48:57.484Z",
  "end": "2020-04-05T15:49:05.000Z"
}
```

## Table Storage structure

### Users table

Contains information about authenticated users.
Only required elements are stored:

| PartitionKey | RowKey | Created | Updated | Name | UserID |  Enabled |
|---|---|---|---|---|---|---|
| [1] | [2] | 2020-04-03T15:51:05.000Z | 2020-05-03T15:51:05.000Z | John Doe | [3] | true |

[1] `"oid"`: The immutable identifier for an object in the Microsoft identity system.

[2] `"tid"`: A GUID that represents the Azure AD tenant that the user is from.
Note: For personal accounts, the value is 9188040d-6c67-4c5b-b112-36a304b66dad.

[3] Unique identifier of user used in other data elements e.g. games.

Note: `Enabled` field is used only if for some reason we need to disable login of certain user.

### UserID2User table

Map from UserID into User record.

| PartitionKey | RowKey | UserPrimaryKey | UserRowKey |
|---|---|---|---|
| [1] | [1] | [2] | [3] |

[1] Unique identifier of user

### Settings table

TBD

### Game table

TBD

| PartitionKey | RowKey | Updated | Data* |
|---|---|---|---|
| [1] | 123-abc-123 | 2020-04-03T15:51:05.000Z | ... |
| [1] | 123-abc-123 | 2020-04-04T15:51:05.000Z | ... |

[1] Unique identifier of user

*) Compressed game data object.
