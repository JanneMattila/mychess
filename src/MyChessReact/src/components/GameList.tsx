import React from "react";

type Game = {
    id: string;
    title: string;
};

type GameListProps = {
    filter: string;
};

type GameListState = {
    games: Game[];
    loading: boolean;
};

export class GameList extends React.Component<GameListProps, GameListState> {
    static displayName = GameList.name;

    constructor(props: GameListProps) {
        super(props);
        this.state = { games: [], loading: true };
    }

    componentDidMount() {
        this.populateGames();
    }

    static renderGames(games: Game[]) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {games.map(game =>
                        <tr key={game.id}>
                            <td>{game.title}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : GameList.renderGames(this.state.games);

        return (
            <div>
                <h1>Games</h1>
                {contents}
            </div>
        );
    }

    async populateGames() {
        const response = await fetch("games");
        const data = await response.json();
        this.setState({ games: data, loading: false });
    }
}
