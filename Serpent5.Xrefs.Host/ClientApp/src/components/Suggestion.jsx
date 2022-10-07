import PropTypes from "prop-types";
import Button from "./Button";

const COPY_NAME = 0;
const COPY_NAME_WITH_TYPE = 1;
const COPY_FULL_NAME = 2;
const COPY_CUSTOM_TEXT = 3;
const COPY_MEMBER_ONLY = 4;

export default function Suggestion({ xrefValue }) {
  return (
    <div className="px-8 py-6">
      <div className="break-all">{xrefValue}</div>

      {navigator.clipboard && (
        <div className="mt-4 md:flex md:items-center">
          <div className="text-center md:mr-4">Copy:</div>
          <div className="mt-2 space-y-2 md:mt-0 md:space-x-2 md:space-y-0">
            <Button onClick={() => copyToClipboard(xrefValue, COPY_NAME)}>
              Name
            </Button>
            <Button
              onClick={() => copyToClipboard(xrefValue, COPY_NAME_WITH_TYPE)}
            >
              Name with Type
            </Button>
            <Button onClick={() => copyToClipboard(xrefValue, COPY_FULL_NAME)}>
              Full Name
            </Button>
            <Button
              onClick={() => copyToClipboard(xrefValue, COPY_CUSTOM_TEXT)}
            >
              Custom Text
            </Button>
            <Button
              onClick={() => copyToClipboard(xrefValue, COPY_MEMBER_ONLY)}
            >
              Member Only
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

Suggestion.propTypes = {
  xrefValue: PropTypes.string.isRequired
};

function copyToClipboard(xrefValue, copyStyle) {
  let xrefMemberValue =
    "xref:" + xrefValue.replace(/\*/g, "%2A").replace(/`/g, "%60");
  let xrefClipboardValue;

  switch (copyStyle) {
    case COPY_NAME:
      xrefClipboardValue = `<${xrefMemberValue}>`;
      break;

    case COPY_NAME_WITH_TYPE:
      xrefClipboardValue = `<${xrefMemberValue}?displayProperty=nameWithType>`;
      break;

    case COPY_FULL_NAME:
      xrefClipboardValue = `<${xrefMemberValue}?displayProperty=fullName>`;
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
