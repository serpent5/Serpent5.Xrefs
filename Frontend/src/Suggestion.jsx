import PropTypes from "prop-types";
import Button from "./Button";

const COPY_NONE = 0;
const COPY_FULL_NAME = 1;
const COPY_NAME_WITH_TYPE = 2;
const COPY_CUSTOM_TEXT = 3;
const COPY_MEMBER_ONLY = 4;

export default function Suggestion({ xrefValue }) {
  return (
    <div className="relative px-8 py-6 cursor-pointer">
      <div className="break-all">{xrefValue}</div>

      {navigator.clipboard && (
        <div className="mt-4 space-x-2">
          <span className="mr-2">Copy:</span>
          <Button onClick={() => copyToClipboard(xrefValue, COPY_NONE)}>None</Button>
          <Button onClick={() => copyToClipboard(xrefValue, COPY_FULL_NAME)}>Full Name</Button>
          <Button onClick={() => copyToClipboard(xrefValue, COPY_NAME_WITH_TYPE)}>Name with Type</Button>
          <Button onClick={() => copyToClipboard(xrefValue, COPY_CUSTOM_TEXT)}>Custom Text</Button>
          <Button onClick={() => copyToClipboard(xrefValue, COPY_MEMBER_ONLY)}>Member Only</Button>
        </div>
      )}
    </div>
  );
}

Suggestion.propTypes = {
  xrefValue: PropTypes.string.isRequired
};

function copyToClipboard(xrefValue, copyStyle) {
  let xrefMemberValue = "xref:" + xrefValue.replace(/\*/g, "%2A").replace(/`/g, "%60");
  let xrefClipboardValue;

  switch (copyStyle) {
    case COPY_NONE:
      xrefClipboardValue = `<${xrefMemberValue}>`;
      break;

    case COPY_FULL_NAME:
      xrefClipboardValue = `<${xrefMemberValue}?displayProperty=fullName>`;
      break;

    case COPY_NAME_WITH_TYPE:
      xrefClipboardValue = `<${xrefMemberValue}?displayProperty=nameWithType>`;
      break;

    case COPY_CUSTOM_TEXT:
      xrefClipboardValue = `[](${xrefMemberValue})`;
      break;

    case COPY_MEMBER_ONLY:
      xrefClipboardValue = xrefMemberValue;
      break;
  }

  navigator.clipboard.writeText(xrefClipboardValue);
}
