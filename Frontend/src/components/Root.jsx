import React from "react";
import { Suggestion } from "./Suggestion";

export class Root extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      searchText: "",
      xrefSuggestions: null,
      isSearching: false
    };
  }

  componentDidMount() {
    if (window.URLSearchParams) {
      const urlSearchParams = new URLSearchParams(location.search);
      const q = urlSearchParams.get("q");

      if (q) {
        this.setState({
          searchText: q
        });

        setTimeout(() => this.onSubmit({ preventDefault: () => {} }));
      }
    }
  }

  render() {
    return (
      <>
        <main className="max-w-screen-lg lg:mx-auto m-4">
          <form onSubmit={e => this.onSubmit(e)}>
            <input
              type="text"
              value={this.state.searchText}
              className="w-full"
              autoFocus
              onChange={e => this.setState({ searchText: e.target.value })}
            />
          </form>

          {this.state.xrefSuggestions?.length > 0 && (
            <ol className="mt-4 space-y-2">
              {this.state.xrefSuggestions.map(x => (
                <li key={x}>
                  <Suggestion xrefValue={x} />
                </li>
              ))}
            </ol>
          )}

          {!this.state.isSearching && this.state.xrefSuggestions?.length === 0 && <p>No Results</p>}
        </main>
      </>
    );
  }

  onSubmit(e) {
    e.preventDefault();

    if (this.state.isSearching) return;
    if (!this.state.searchText) return;

    if (history.replaceState && window.URLSearchParams) {
      history.replaceState(
        null,
        null,
        "?" +
          new URLSearchParams({
            q: this.state.searchText
          }).toString()
      );
    }

    this.setState({
      xrefSuggestions: [],
      isSearching: true
    });

    var apiURL = new URL(location.origin + "/api/Suggestions");

    apiURL.searchParams.append("q", this.state.searchText);

    fetch(apiURL)
      .then(apiResponse => {
        if (!apiResponse.ok) {
          throw new Error();
        }

        return apiResponse.json();
      })
      .then(xrefSuggestions =>
        this.setState({
          xrefSuggestions: xrefSuggestions.map(x => x.replace(/\*/g, "%2A").replace(/`/g, "%60")),
          isSearching: false
        })
      )
      .catch(() =>
        // TODO: Handle error responses.
        this.setState({ xrefSuggestions: [], isSearching: false })
      );
  }
}
