import { PropTypes } from "prop-types";
import Suggestion from "./Suggestion";

export default function Results({ xrefSuggestions }) {
  if (xrefSuggestions.length === 0) {
    return (
      <div className="mt-8 border border-gray-500 bg-white shadow-xl">
        <p className="px-8 py-6">No Results</p>
      </div>
    );
  }

  return (
    <ol className="mt-6 divide-y divide-gray-500 border border-gray-500 bg-white shadow-xl">
      {xrefSuggestions.map(x => (
        <li key={x.uid}>
          <Suggestion xrefValue={x.uid} />
        </li>
      ))}
    </ol>
  );
}

Results.propTypes = {
  xrefSuggestions: PropTypes.array.isRequired
};
