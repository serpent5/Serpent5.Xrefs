import React from "react";
import Results from "./Results";

export default class Root extends React.Component {
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
        this.setState({ searchText: q });

        setTimeout(() => this.fetchSuggestions());
      }
    }
  }

  render() {
    const { searchText, isSearching, xrefSuggestions } = this.state;

    return (
      <>
        <main className="mx-4 my-6 max-w-screen-xl md:mx-auto md:my-8">
          <form className="shadow-xl" onSubmit={e => this.onSubmit(e)}>
            <div className="relative">
              <input
                type="text"
                placeholder="Enter a namespace, type, or member to search the .NET API reference (e.g. AddSingleton)"
                value={searchText}
                autoFocus
                className="w-full px-12 py-4"
                onChange={e => this.setState({ searchText: e.target.value })}
                readOnly={isSearching}
              />

              <div className="pointer-events-none absolute top-1/2 left-4 -translate-y-1/2 text-gray-500">
                {isSearching ? (
                  <svg
                    className="w-6 animate-spin"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                  >
                    <circle
                      className="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      strokeWidth="4"
                    ></circle>
                    <path
                      className="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    ></path>
                  </svg>
                ) : (
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    className="w-6"
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
                )}
              </div>
            </div>
          </form>

          {xrefSuggestions && <Results xrefSuggestions={xrefSuggestions} />}
        </main>
      </>
    );
  }

  onSubmit(e) {
    e.preventDefault();
    this.fetchSuggestions();
  }

  async fetchSuggestions() {
    if (this.state.isSearching) return;
    if (!this.state.searchText) return;

    const searchText = this.state.searchText.trim();

    if (!searchText) return;

    if (history.replaceState && window.URLSearchParams) {
      history.replaceState(
        null,
        null,
        "?" + new URLSearchParams({ q: searchText }).toString()
      );
    }

    this.setState({ xrefSuggestions: null, searchText, isSearching: true });

    var apiURL = new URL(location.origin + "/api/Autocomplete");

    apiURL.searchParams.append("text", searchText);

    let xrefSuggestions = [];

    try {
      const apiResponse = await fetch(apiURL);

      if (apiResponse.ok) {
        xrefSuggestions = await apiResponse.json();
      }
    } catch {
      // TODO: Handle error responses.
    }

    this.setState({ xrefSuggestions, isSearching: false });
  }
}
