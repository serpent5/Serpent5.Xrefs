import React from "react";

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
      .then(xrefSuggestions => this.setState({ xrefSuggestions, isSearching: false }))
      .catch(() =>
        // TODO: Handle error responses.
        this.setState({ xrefSuggestions: [], isSearching: false })
      );
  }

  onSuggestionClick(xrefSuggestion) {
    navigator.clipboard.writeText(`<xref:${xrefSuggestion.replace(/\*/g, "%2A").replace(/`/g, "%60")}>`);
  }

  render() {
    return (
      <>
        <main className="max-w-screen-lg lg:mx-auto mt-4 mx-4">
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
                <li key={x} className="flex items-center p-2 border break-all">
                  {navigator.clipboard && (
                    <button className="mr-1" onClick={() => this.onSuggestionClick(x)}>
                      <svg
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                        xmlns="http://www.w3.org/2000/svg"
                        className="w-10 p-2 text-gray-500"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth="2"
                          d="M8 5H6a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2v-1M8 5a2 2 0 002 2h2a2 2 0 002-2M8 5a2 2 0 012-2h2a2 2 0 012 2m0 0h2a2 2 0 012 2v3m2 4H10m0 0l3-3m-3 3l3 3"
                        ></path>
                      </svg>
                    </button>
                  )}
                  <span className="w-full">{x}</span>
                </li>
              ))}
            </ol>
          )}

          {!this.state.isSearching && this.state.xrefSuggestions?.length === 0 && <p>No Results</p>}
        </main>
      </>
    );
  }
}
