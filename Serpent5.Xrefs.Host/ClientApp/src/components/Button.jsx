import PropTypes from "prop-types";

export default function Button({ children, onClick }) {
  return (
    <button
      type="button"
      className="px-4 py-2 w-full border-2 bg-gray-200 rounded-md shadow-sm hover:bg-gray-400 transition-colors md:w-auto"
      onClick={onClick}
    >
      {children}
    </button>
  );
}

Button.propTypes = {
  children: PropTypes.node.isRequired,
  onClick: PropTypes.func
};
