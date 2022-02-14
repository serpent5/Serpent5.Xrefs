import PropTypes from "prop-types";

export default function Button({ children, onClick }) {
  return (
    <button
      type="button"
      className="px-4 py-2 border-2 bg-gray-200 rounded-md shadow-sm hover:bg-gray-400 transition-colors"
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
