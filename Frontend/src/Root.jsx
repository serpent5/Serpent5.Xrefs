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
        <main className="max-w-screen-xl lg:mx-auto my-12 mx-4">
          <form onSubmit={e => this.onSubmit(e)}>
            <div className="relative">
              <input
                type="text"
                value={this.state.searchText}
                className="w-full px-14 py-4 rounded-lg"
                autoFocus
                onChange={e => this.setState({ searchText: e.target.value })}
              />

              <div className="absolute top-1/2 -translate-y-1/2 left-4 text-gray-500 pointer-events-none">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  className="w-8"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                  />
                </svg>
              </div>
            </div>
          </form>

          {this.state.xrefSuggestions?.length > 0 && (
            <ol className="mt-8 bg-white border border-gray-500 divide-y divide-gray-500 rounded-md">
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
